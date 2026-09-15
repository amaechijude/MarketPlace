using MarketPlace.Notification;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.MapDefaultEndpoints();
host.Run();
