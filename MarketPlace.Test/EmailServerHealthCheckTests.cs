using System.Net;
using System.Net.Sockets;
using MarketPlace.Api.Infrastucture.Email;
using Microsoft.Extensions.Options;

namespace MarketPlace.Test;

public sealed class EmailServerHealthCheckTests
{
    [Fact]
    public async Task PingAsync_succeeds_when_email_server_accepts_connections()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var settings = Options.Create(
            new SmtpSettings
            {
                Host = IPAddress.Loopback.ToString(),
                Port = ((IPEndPoint)listener.LocalEndpoint).Port,
            }
        );
        var healthCheck = new EmailServerHealthCheck(settings);

        Task pingTask = healthCheck.PingAsync(CancellationToken.None);
        using TcpClient connection = await listener.AcceptTcpClientAsync();

        await pingTask;
    }
}
