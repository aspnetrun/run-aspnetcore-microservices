using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace BuildingBlocks.OpenTelemetry;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        string serviceVersion = "1.0.0")
    {
        var otelEndpoint = configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317";
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["environment"] = environment,
                    ["deployment.region"] = "local",
                    ["service.instance.id"] = Environment.MachineName
                }))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(serviceName)
                .AddOtlpExporter(opts => opts.Endpoint = new Uri(otelEndpoint))
                .SetSampler(new AlwaysOnSampler()));

        // Register ActivitySource for custom instrumentation
        services.AddSingleton(new ActivitySource(serviceName));

        return services;
    }

    public static IServiceCollection AddOpenTelemetryHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var otelEndpoint = configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317";

        services.AddHealthChecks()
            .AddCheck("otel-collector", () =>
            {
                try
                {
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = client.GetAsync($"{otelEndpoint}/").Result;
                    return response.IsSuccessStatusCode ? 
                        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy() : 
                        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy();
                }
                catch
                {
                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy();
                }
            });

        return services;
    }
}
