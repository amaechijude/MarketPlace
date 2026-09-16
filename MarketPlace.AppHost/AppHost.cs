var builder = DistributedApplication.CreateBuilder(args);
var rabitmq = builder.AddRabbitMQ("quee");
var redis = builder
    .AddRedis("Redis")
    .WithDataVolume("marketplace-redis-data")
    .WithImage("redis:8.6");

builder.AddProject<Projects.MarketPlace_Api>("marketplace-api").WaitFor(redis).WithReference(redis);

builder.AddProject<Projects.MarketPlace_Notification>("marketplace-notification");

builder.Build().Run();
