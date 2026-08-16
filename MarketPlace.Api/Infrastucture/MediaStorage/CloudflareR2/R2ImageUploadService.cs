using Amazon.S3;
using Amazon.S3.Model;
using MarketPlace.Api.Common.Extensions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SkiaSharp;

namespace MarketPlace.Api.Infrastucture.MediaStorage.CloudflareR2;

public sealed class R2ImageUploadService(IAmazonS3 s3Client, IOptions<R2Options> settings)
    : IRequestHandler
{
    private readonly R2Options _settings = settings.Value;

    public async Task<ImageUploadResult> UploadImageAsync(
        IFormFile file,
        string folder,
        CancellationToken cancellationToken
    )
    {
        await using var stream = file.OpenReadStream();
        using var bitmap = SKBitmap.Decode(stream);

        using var image = SKImage.FromBitmap(bitmap);

        using var data = image.Encode(SKEncodedImageFormat.Webp, 70);
        using var compressesd = new MemoryStream();
        await data.AsStream().CopyToAsync(compressesd, cancellationToken);

        var fileKey = $"{folder}/{Guid.NewGuid()}.webp";

        var request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = fileKey,
            InputStream = compressesd,
            ContentType = "image/webp",
            CannedACL = S3CannedACL.PublicRead,
            DisableDefaultChecksumValidation = true,
            DisablePayloadSigning = true,
        };

        await s3Client.PutObjectAsync(request, cancellationToken);

        return new ImageUploadResult($"{_settings.PublicBaseUrl.TrimEnd('/')}/{fileKey}", fileKey);
    }

    public async Task<ImageUploadResult[]> UploadImageAsync(
        IEnumerable<IFormFile> files,
        string folder,
        CancellationToken cancellationToken
    ) => [.. await Task.WhenAll(files.Select(f => UploadImageAsync(f, folder, cancellationToken)))];

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
    )
    {
        await Task.WhenAll(fileKeys.Select(f => DeleteImageAsync(f, cancellationToken)));
    }
}

public sealed record ImageUploadResult(string FileUrl, string FileKey);
