using System.Threading.Channels;
using Kickify.Application.Abstractions.Services;

namespace Kickify.Infrastructure.Services;

public sealed class SystemLogQueue : ISystemLogQueue
{
    private readonly Channel<SystemLogQueueItem> _channel;

    public SystemLogQueue()
    {
        var options = new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<SystemLogQueueItem>(options);
    }

    public bool TryEnqueue(SystemLogQueueItem item) => _channel.Writer.TryWrite(item);

    public ChannelReader<SystemLogQueueItem> Reader => _channel.Reader;
}
