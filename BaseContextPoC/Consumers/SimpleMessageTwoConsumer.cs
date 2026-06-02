using MassTransit;

namespace BaseContextPoC;

public class SimpleMessageTwoConsumer : IConsumer<SimpleMessageTwo>
{
    public Task Consume(ConsumeContext<SimpleMessageTwo> context)
    {
        Console.WriteLine($"Consumed Simple Message Two: {context.Message.Name}, {context.Message.Description}");
        return Task.CompletedTask;
    }
}