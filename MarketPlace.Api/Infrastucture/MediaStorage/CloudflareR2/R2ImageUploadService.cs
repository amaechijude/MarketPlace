using Amazon.S3;
using Amazon.S3.Model;
using MarketPlace.Api.Common.Extensions;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace MarketPlace.Api.Infrastucture.MediaStorage.CloudflareR2;

public sealed record UploadImageRequest(IFormFile File, string Folder, bool IsThumbnail);

public sealed class R2ImageUploadService(IAmazonS3 s3Client, IOptions<R2Options> settings)
    : ISingletonMarker
{
    private readonly R2Options _settings = settings.Value;

    public async Task<ImageUploadResult> UploadImageAsync(
        UploadImageRequest request,
        CancellationToken cancellationToken
    )
    {
        await using var stream = request.File.OpenReadStream();
        using var bitmap = SKBitmap.Decode(stream);

        using var image = SKImage.FromBitmap(bitmap);

        using var data = image.Encode(SKEncodedImageFormat.Webp, 70);
        using var compressesd = new MemoryStream();
        await data.AsStream().CopyToAsync(compressesd, cancellationToken);

        var fileKey = $"{request.Folder}/{Guid.NewGuid()}.webp";

        var r2Request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = fileKey,
            InputStream = compressesd,
            ContentType = "image/webp",
            CannedACL = S3CannedACL.PublicRead,
            DisableDefaultChecksumValidation = true,
            DisablePayloadSigning = true,
        };

        await s3Client.PutObjectAsync(r2Request, cancellationToken);

        return new ImageUploadResult(
            $"{_settings.PublicBaseUrl.TrimEnd('/')}/{fileKey}",
            request.IsThumbnail,
            fileKey
        );
    }

    public async Task<List<ImageUploadResult>> UploadImageAsync(
        IEnumerable<UploadImageRequest> requests,
        CancellationToken cancellationToken
    ) => [.. await Task.WhenAll(requests.Select(r => UploadImageAsync(r, cancellationToken)))];

    public async Task DeleteImageAsync(string fileKey, CancellationToken cancellationToken)
    {
        await s3Client.DeleteObjectAsync(
            new DeleteObjectRequest { BucketName = _settings.BucketName, Key = fileKey },
            cancellationToken
        );
    }

    public async Task DeleteImageAsync(
        IEnumerable<string> fileKeys,
        CancellationToken cancellationToken
    ) => await Task.WhenAll(fileKeys.Select(f => DeleteImageAsync(f, cancellationToken)));
}

public sealed record ImageUploadResult(string FileUrl, bool Isthubnail, string FileKey);
