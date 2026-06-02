using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Test.Module.Entities;

namespace Test.Module;

public class ModuleWorker : BackgroundService
{

    private readonly IServiceProvider _serviceProvider;
    public ModuleWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        
        await using var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        
        var messageCounter = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            var entity = new ModuleEntity() { Name = $"MassTransit Outbox Message {messageCounter}"};
            //dbContext.Set<ModuleEntity>().Add(entity);

            Console.WriteLine($"Publishing ModuleBasedMessage: {entity.Name}");
            await publishEndpoint.Publish(new ModuleBasedMessage() { Id = entity.Id, Name = entity.Name}, stoppingToken);
            
            await dbContext.SaveChangesAsync(stoppingToken);
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            messageCounter++;
        }
    }
}