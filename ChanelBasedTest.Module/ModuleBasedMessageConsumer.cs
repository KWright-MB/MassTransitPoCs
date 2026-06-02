using MassTransit;

namespace ChanelBasedTest.Module;

public class ModuleBasedMessageConsumer : IConsumer<ModuleBasedMessage>
{
    public Task Consume(ConsumeContext<ModuleBasedMessage> context)
    {
        Console.WriteLine($"Consumed Module Based Message: {context.Message.Name}");
        return Task.CompletedTask;
    }
}