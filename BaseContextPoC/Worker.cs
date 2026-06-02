using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BaseContextPoC;

public class Worker : IHostedService
{

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDbContextFactory<SimpleContextOne> _simpleContextOneFactory;
    private readonly IDbContextFactory<SimpleContextTwo> _simpleContextTwoFactory;
    
    public Worker(IServiceScopeFactory scopeFactory,
        IDbContextFactory<SimpleContextOne> simpleContextOneFactory,
        IDbContextFactory<SimpleContextTwo> simpleContextTwoFactory)
    {
        _scopeFactory = scopeFactory;
        _simpleContextOneFactory = simpleContextOneFactory;
        _simpleContextTwoFactory = simpleContextTwoFactory;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await EnsureMigratedAsync(cancellationToken);

        _ = Task.Run(async () =>
        {
            var messageCounter = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                await OutboxFromContextOne(messageCounter, cancellationToken);
                await Task.Delay(10000, cancellationToken);
                messageCounter++;
            }
        }, cancellationToken);

        _ = Task.Run(async () =>
        {
            var messageCounter = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(5000, cancellationToken);
                await OutboxFromContextTwo(messageCounter, cancellationToken);
                await Task.Delay(5000, cancellationToken);
                messageCounter++;
            }
        }, cancellationToken);
    }

    private async Task OutboxFromContextTwo(int messageCounter, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            // Resolve the concrete context first so it's initialized in the scope
            var context = scope.ServiceProvider.GetRequiredService<SimpleContextTwo>();
            
            // We use the shared base context registered with MassTransit.
            // Since we registered AddEntityFrameworkOutbox<MassTransitOutboxingContext>,
            // MT will resolve MassTransitOutboxingContext from the scope.
            // To make this work for SimpleContextTwo, we'd need MassTransitOutboxingContext to resolve to 'context' (SimpleContextTwo).
            
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            var entity = new SimpleEntityTwo() { Name = $"MassTransit Outbox Message {messageCounter}", Description = $"Message Number {messageCounter}"};
            context.SimpleEntityTwo.Add(entity);

            Console.WriteLine($"Publishing Message Two: {entity.Name}");
            await publishEndpoint.Publish(new SimpleMessageTwo() { Id = entity.Id, Name = entity.Name, Description = entity.Description}, cancellationToken);
            
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            Console.WriteLine($"Saved changes for Context Two, message {messageCounter}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OutboxFromContextTwo: {ex.Message}");
        }
    }

    private async Task OutboxFromContextOne(int messageCounter, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SimpleContextOne>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            var entity = new SimpleEntityOne() { Name = $"MassTransit Outbox Message {messageCounter}" };
            context.SimpleEntityOne.Add(entity);

            Console.WriteLine($"Publishing Message One: {entity.Name}");
            await publishEndpoint.Publish(new SimpleMessageOne() { Id = entity.Id, Name = entity.Name }, cancellationToken);
            
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            Console.WriteLine($"Saved changes for Context One, message {messageCounter}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OutboxFromContextOne: {ex.Message}");
        }
    }

    private async Task EnsureMigratedAsync(CancellationToken cancellationToken)
    {
        await using var contextOne = await _simpleContextOneFactory.CreateDbContextAsync(cancellationToken);
        await using var contextTwo = await _simpleContextTwoFactory.CreateDbContextAsync(cancellationToken);
        
        await contextOne.Database.MigrateAsync(cancellationToken: cancellationToken);
        await contextTwo.Database.MigrateAsync(cancellationToken: cancellationToken);
        
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}