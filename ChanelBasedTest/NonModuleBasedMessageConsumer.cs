using MassTransit;

namespace ChanelBasedTest;

public class NonModuleBasedMessageConsumer : IConsumer<NonModuleBasedMessage>
{
    public Task Consume(ConsumeContext<NonModuleBasedMessage> context)
    {
        Console.WriteLine($"Consumed Non Module Based Message: {context.Message.Name}");
        return Task.CompletedTask;
    }
}