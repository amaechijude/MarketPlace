var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MarketPlace_Api>("marketplace-api");

builder.Build().Run();
