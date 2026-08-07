// using System.Threading.Channels;

// namespace GitgBrand.Api.Infrastructure.Email;

// public sealed class EnqueOtpEmailService(Channel<OtpEmailRequest> emaiChannel)
// {
//     public async Task EnqueMail(OtpEmailRequest request, CancellationToken ct)
//     {
//         await emaiChannel.Writer.WriteAsync(request, ct);
//     }
// }
