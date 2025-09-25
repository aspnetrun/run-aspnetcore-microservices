using Microsoft.AspNetCore.RateLimiting;
using BuildingBlocks.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry services
builder.Services.AddOpenTelemetryServices(builder.Configuration, "yarp-gateway", "1.0.0");

// Add services to the container.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.Window = TimeSpan.FromSeconds(10);
        options.PermitLimit = 5;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRateLimiter();



app.MapReverseProxy();

app.Run();
