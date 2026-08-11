
namespace MarketPlace.Api.Common.Extensions;

public static class HttpContextExtensions
{
    public static object GetClientIp(this HttpContext context)
    {
        // cloud flare
        string? cloudflareIp = context.Request.Headers["cf-connecting-ip"];


        // nginx
        string? nginxIp = context.Request.Headers["x-real-ip"];

        // x forwaderd
        string? forwarded = context.Request.Headers["x-forwarded-for"];


        // fall back
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();

        return new { cloudflareIp, nginxIp, forwarded, remoteIp };
    }
}