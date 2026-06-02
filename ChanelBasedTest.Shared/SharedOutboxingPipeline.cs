using System.Threading.Channels;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace ChanelBasedTest.Shared;

public class SharedOutboxingPipeline : BackgroundService, ISharedOutboxingPipeline
{
    private readonly ChannelReader<object> _channelReader;
    private readonly ChannelWriter<object> _channelWriter;
    private readonly IPublishEndpoint _publishEndpoint;
    
    public SharedOutboxingPipeline(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
        
        var channel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = true
        }); 
        
        _channelReader = channel.Reader;
        _channelWriter = channel.Writer;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            while (await _channelReader.WaitToReadAsync(stoppingToken))
            {
                while (_channelReader.TryRead(out var message))
                {
                    await _publishEndpoint.Publish(message, stoppingToken);
                }
            }
        }
    }

    public ChannelWriter<object> ChannelWriter => _channelWriter;
}

public interface ISharedOutboxingPipeline
{
    ChannelWriter<object> ChannelWriter { get; }
}