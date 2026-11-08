using System.Threading.Channels;
using MarketPlace.Api.Common.Extensions;

namespace MarketPlace.Api.Infrastucture.OtpService;

public sealed class OtpEmailQueue : ISingletonMarker
{
    private readonly Channel<OtpEmailRequest> _channel = Channel.CreateBounded<OtpEmailRequest>(
        new BoundedChannelOptions(10_000) { FullMode = BoundedChannelFullMode.Wait }
    );

    public ValueTask EnqueueAsync(OtpEmailRequest msg, CancellationToken ct = default) =>
        _channel.Writer.WriteAsync(msg, ct);

    public IAsyncEnumerable<OtpEmailRequest> ReadAllAsync(CancellationToken ct) =>
        _channel.Reader.ReadAllAsync(ct);
}
