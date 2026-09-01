var builder = DistributedApplication.CreateBuilder(args);
var redis = builder.AddRedis("Redis");
builder.AddProject<Projects.MarketPlace_Api>("marketplace-api").WaitFor(redis).WithReference(redis);

builder.Build().Run();
