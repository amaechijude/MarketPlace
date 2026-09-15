using MarketPlace.Notification;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();
var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
