using MassTransit;

namespace BaseContextPoC;

public class SimpleMessageOneConsumer : IConsumer<SimpleMessageOne>
{
    public Task Consume(ConsumeContext<SimpleMessageOne> context)
    {
        Console.WriteLine($"Consumed Simple Message One: {context.Message.Name}");
        return Task.CompletedTask;
    }
}