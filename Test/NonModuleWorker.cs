using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Test;

public class NonModuleWorker : BackgroundService
{

    private readonly IServiceProvider _serviceProvider;
    
    public NonModuleWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messageCounter = 0;
        await using var scope = _serviceProvider.CreateAsyncScope();
        
        await using var dbContext = scope.ServiceProvider.GetRequiredService<WillowContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            
            var entity = new WillowEntity() { Name = $"MassTransit Outbox Message {messageCounter}"};
            dbContext.WillowEntities.Add(entity);

            Console.WriteLine($"Publishing NonModuleBasedMessage: {entity.Name}");
            await publishEndpoint.Publish(new NonModuleBasedMessage() { Id = entity.Id, Name = entity.Name}, stoppingToken);
            
            await dbContext.SaveChangesAsync(stoppingToken);
        
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            messageCounter++;
        }
    }
}