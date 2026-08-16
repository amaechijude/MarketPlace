using Amazon.Runtime;
using Amazon.S3;
using MarketPlace.Api.Infrastucture.MediaStorage.CloudflareR2;
using Microsoft.Extensions.Options;

namespace MarketPlace.Api.Infrastucture.MediaStorage;

public static class MediaStorageServiceCollection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMediaStorageInfrastructure(IConfiguration configuration) =>
            services
            .AddCloudflareR2Settings(configuration)
            .AddAmazonS3Config();



        private IServiceCollection AddCloudflareR2Settings(IConfiguration configuration)
        {
            DotNetEnv.Env.TraversePath().Load();

            services
                .AddOptions<R2Options>()
                .Bind(configuration.GetRequiredSection(nameof(R2Options)))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services;
        }

        private IServiceCollection AddAmazonS3Config()
        {
            services.AddSingleton<IAmazonS3>(provider =>
            {
                var settingns = provider.GetRequiredService<IOptions<R2Options>>().Value;

                var credentials = new BasicAWSCredentials(accessKey: settingns.AccessKeyId, secretKey: settingns.SecretAccessKey);

                var config = new AmazonS3Config
                {
                    ServiceURL = settingns.ServiceUrl,
                    ForcePathStyle = true,
                };

                return new AmazonS3Client(credentials, config);
            });


            return services;
        }
    }
}
