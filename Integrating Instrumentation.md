# Integrating OpenTelemetry, Jaeger, and Prometheus in C# Projects

## Overview
This guide provides a comprehensive approach to implementing observability in C# applications using OpenTelemetry for telemetry collection, Jaeger for distributed tracing, and Prometheus for metrics collection and monitoring.

## Architecture Components

### 1. OpenTelemetry Collector
- **Service**: OpenTelemetry Collector (Docker container or standalone)
- **Configuration**: YAML-based configuration
- **Port**: 4317 (gRPC), 4318 (HTTP)

### 2. Jaeger Tracing
- **Service**: Jaeger All-in-One or distributed setup
- **Port**: 16686 (UI), 14268 (HTTP), 14250 (gRPC)

### 3. Prometheus Metrics
- **Service**: Prometheus server
- **Port**: 9090 (UI and metrics endpoint)

## C# Implementation

### NuGet Packages Required

```xml
<PackageReference Include="OpenTelemetry" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Api" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.SqlClient" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Exporter.Jaeger" Version="1.7.0" />
```

### Program.cs Configuration

```csharp
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry services
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "YourServiceName", 
                   serviceVersion: "1.0.0"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddSource("YourNamespace.*")
        .SetSampler(new AlwaysOnSampler())
        .AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri("http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter()
        .AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri("http://localhost:4317");
        }));

var app = builder.Build();

// Configure Prometheus metrics endpoint
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();
```

### Docker Compose Setup

```yaml
version: '3.8'
services:
  otel-collector:
    image: otel/opentelemetry-collector-contrib:0.81.0
    command: ["--config=/etc/otel-collector-config.yaml"]
    volumes:
      - ./otel-collector-config.yaml:/etc/otel-collector-config.yaml
    ports:
      - "4317:4317"   # OTLP gRPC
      - "4318:4318"   # OTLP HTTP
      - "8889:8889"   # Prometheus metrics

  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"  # UI
      - "14268:14268"  # HTTP
      - "14250:14250"  # gRPC

  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
```

### OpenTelemetry Collector Configuration

```yaml
# otel-collector-config.yaml
receivers:
  otlp:
    protocols:
      grpc:
      http:

exporters:
  prometheus:
    endpoint: "0.0.0.0:8889"
    send_timestamps: true
    resource_to_telemetry_conversion:
      enabled: true
  jaeger:
    endpoint: jaeger:14250
    tls:
      insecure: true

processors:
  batch:
  resource:
    attributes:
      - key: environment
        value: "development"
        action: upsert

connectors:
  spanmetrics:
    namespace: span.metrics
    histogram:
      explicit:
        buckets: [100us, 1ms, 2ms, 6ms, 10ms, 100ms, 250ms]
    dimensions:
      - name: http.status_code
      - name: http.method

service:
  extensions: [health_check, pprof, zpages]
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch, resource]
      exporters: [spanmetrics, jaeger]
    metrics:
      receivers: [otlp, spanmetrics]
      processors: [batch, resource]
      exporters: [prometheus]
```

### Prometheus Configuration

```yaml
# prometheus.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'otel-collector'
    scrape_interval: 1s
    static_configs:
      - targets: ['otel-collector:8889']
```

## Best Practices for Instrumentation

### 1. API Controllers and Endpoints

**Always instrument these areas:**
- HTTP request/response cycles
- Authentication and authorization
- Input validation
- Business logic operations
- Error handling

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ActivitySource _activitySource;
    private readonly Counter<long> _userCreationCounter;
    private readonly Histogram<double> _userCreationDuration;

    public UsersController(IMeterFactory meterFactory, ActivitySource activitySource)
    {
        _activitySource = activitySource;
        var meter = meterFactory.Create("UsersController");
        _userCreationCounter = meter.CreateCounter<long>("user_creation_total");
        _userCreationDuration = meter.CreateHistogram<double>("user_creation_duration_seconds");
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        using var activity = _activitySource.StartActivity("CreateUser");
        activity?.SetTag("user.email", request.Email);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Your business logic here
            var user = await _userService.CreateUserAsync(request);
            
            stopwatch.Stop();
            _userCreationCounter.Add(1);
            _userCreationDuration.Record(stopwatch.Elapsed.TotalSeconds);
            
            activity?.SetTag("user.id", user.Id);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _userCreationDuration.Record(stopwatch.Elapsed.TotalSeconds);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            
            throw;
        }
    }
}
```

### 2. Database Access

**Instrument these database operations:**
- Connection management
- Query execution
- Transaction handling
- Connection pooling metrics

```csharp
public class UserRepository : IUserRepository
{
    private readonly ActivitySource _activitySource;
    private readonly Histogram<double> _queryDuration;
    private readonly Counter<long> _queryCount;

    public UserRepository(IMeterFactory meterFactory, ActivitySource activitySource)
    {
        _activitySource = activitySource;
        var meter = meterFactory.Create("UserRepository");
        _queryDuration = meter.CreateHistogram<double>("database_query_duration_seconds");
        _queryCount = meter.CreateCounter<long>("database_query_total");
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        using var activity = _activitySource.StartActivity("Database.GetUserById");
        activity?.SetTag("db.operation", "SELECT");
        activity?.SetTag("db.table", "Users");
        activity?.SetTag("db.user_id", id);

        var stopwatch = Stopwatch.StartNew();
        _queryCount.Add(1);

        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            stopwatch.Stop();
            _queryDuration.Record(stopwatch.Elapsed.TotalSeconds);

            if (user == null)
            {
                activity?.SetTag("db.result", "not_found");
            }
            else
            {
                activity?.SetTag("db.result", "found");
            }

            return user;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _queryDuration.Record(stopwatch.Elapsed.TotalSeconds);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            
            throw;
        }
    }
}
```

### 3. External Service Calls

**Instrument these HTTP operations:**
- API calls to external services
- Retry logic
- Circuit breaker patterns
- Response time tracking

```csharp
public class ExternalApiService : IExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly ActivitySource _activitySource;
    private readonly Histogram<double> _apiCallDuration;
    private readonly Counter<long> _apiCallCount;

    public ExternalApiService(HttpClient httpClient, IMeterFactory meterFactory, ActivitySource activitySource)
    {
        _httpClient = httpClient;
        _activitySource = activitySource;
        var meter = meterFactory.Create("ExternalApiService");
        _apiCallDuration = meter.CreateHistogram<double>("external_api_call_duration_seconds");
        _apiCallCount = meter.CreateCounter<long>("external_api_call_total");
    }

    public async Task<ApiResponse> CallExternalApiAsync(string endpoint, object data)
    {
        using var activity = _activitySource.StartActivity("ExternalApi.Call");
        activity?.SetTag("http.url", endpoint);
        activity?.SetTag("http.method", "POST");

        var stopwatch = Stopwatch.StartNew();
        _apiCallCount.Add(1);

        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            
            stopwatch.Stop();
            _apiCallDuration.Record(stopwatch.Elapsed.TotalSeconds);

            activity?.SetTag("http.status_code", (int)response.StatusCode);
            activity?.SetTag("http.response_size", response.Content.Headers.ContentLength);

            if (response.IsSuccessStatusCode)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                activity?.SetStatus(ActivityStatusCode.Error, $"HTTP {response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<ApiResponse>();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _apiCallDuration.Record(stopwatch.Elapsed.TotalSeconds);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            
            throw;
        }
    }
}
```

### 4. Background Services and Workers

**Instrument these long-running operations:**
- Job processing
- Queue consumption
- Scheduled tasks
- Batch operations

```csharp
public class BackgroundWorkerService : BackgroundService
{
    private readonly ActivitySource _activitySource;
    private readonly Histogram<double> _jobProcessingDuration;
    private readonly Counter<long> _jobsProcessed;
    private readonly Gauge<long> _queueSize;

    public BackgroundWorkerService(IMeterFactory meterFactory, ActivitySource activitySource)
    {
        _activitySource = activitySource;
        var meter = meterFactory.Create("BackgroundWorker");
        _jobProcessingDuration = meter.CreateHistogram<double>("job_processing_duration_seconds");
        _jobsProcessed = meter.CreateCounter<long>("jobs_processed_total");
        _queueSize = meter.CreateGauge<long>("queue_size");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = _activitySource.StartActivity("BackgroundWorker.ProcessJobs");
            
            try
            {
                var jobs = await _queueService.GetPendingJobsAsync();
                _queueSize.Set(jobs.Count);

                foreach (var job in jobs)
                {
                    await ProcessJobAsync(job, stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.RecordException(ex);
                
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ProcessJobAsync(Job job, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("BackgroundWorker.ProcessJob");
        activity?.SetTag("job.id", job.Id);
        activity?.SetTag("job.type", job.Type);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _jobProcessor.ProcessAsync(job);
            
            stopwatch.Stop();
            _jobProcessingDuration.Record(stopwatch.Elapsed.TotalSeconds);
            _jobsProcessed.Add(1);

            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _jobProcessingDuration.Record(stopwatch.Elapsed.TotalSeconds);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            
            throw;
        }
    }
}
```

### 5. Authentication and Authorization

**Instrument these security operations:**
- Login attempts
- Token validation
- Permission checks
- Security events

```csharp
public class AuthService : IAuthService
{
    private readonly ActivitySource _activitySource;
    private readonly Counter<long> _loginAttempts;
    private readonly Counter<long> _failedLogins;
    private readonly Histogram<double> _tokenValidationDuration;

    public AuthService(IMeterFactory meterFactory, ActivitySource activitySource)
    {
        _activitySource = activitySource;
        var meter = meterFactory.Create("AuthService");
        _loginAttempts = meter.CreateCounter<long>("login_attempts_total");
        _failedLogins = meter.CreateCounter<long>("failed_logins_total");
        _tokenValidationDuration = meter.CreateHistogram<double>("token_validation_duration_seconds");
    }

    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        using var activity = _activitySource.StartActivity("Auth.Authenticate");
        activity?.SetTag("auth.username", username);
        activity?.SetTag("auth.method", "password");

        _loginAttempts.Add(1);

        try
        {
            var result = await _authProvider.AuthenticateAsync(username, password);
            
            if (result.Success)
            {
                activity?.SetTag("auth.result", "success");
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                _failedLogins.Add(1);
                activity?.SetTag("auth.result", "failed");
                activity?.SetTag("auth.failure_reason", result.ErrorMessage);
                activity?.SetStatus(ActivityStatusCode.Error, result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _failedLogins.Add(1);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            
            throw;
        }
    }
}
```

## Configuration Best Practices

### 1. Environment-Specific Configuration

```csharp
// appsettings.Development.json
{
  "OpenTelemetry": {
    "Tracing": {
      "Sampler": "AlwaysOn",
      "Exporter": "Jaeger",
      "JaegerEndpoint": "http://localhost:14268"
    },
    "Metrics": {
      "Exporter": "Prometheus",
      "PrometheusPort": 9090
    }
  }
}

// appsettings.Production.json
{
  "OpenTelemetry": {
    "Tracing": {
      "Sampler": "TraceIdRatioBased",
      "SamplingRatio": 0.1,
      "Exporter": "OTLP",
      "OTLPEndpoint": "http://otel-collector:4317"
    },
    "Metrics": {
      "Exporter": "OTLP",
      "OTLPEndpoint": "http://otel-collector:4317"
    }
  }
}
```

### 2. Sampling Configuration

```csharp
// For development
.SetSampler(new AlwaysOnSampler())

// For production
.SetSampler(new TraceIdRatioBasedSampler(0.1)) // 10% of traces

// Custom sampling based on business logic
.SetSampler(new CustomSampler())
```

### 3. Resource Attributes

```csharp
.ConfigureResource(resource => resource
    .AddService(serviceName: "YourServiceName", 
               serviceVersion: "1.0.0")
    .AddAttributes(new Dictionary<string, object>
    {
        ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
        ["deployment.region"] = Environment.GetEnvironmentVariable("DEPLOYMENT_REGION"),
        ["service.instance.id"] = Environment.MachineName
    }))
```

## Monitoring and Alerting

### 1. Key Metrics to Monitor

- **Application Metrics**:
  - Request rate and response time
  - Error rate and error types
  - Active users and sessions

- **Infrastructure Metrics**:
  - CPU and memory usage
  - Database connection pool status
  - External API response times

- **Business Metrics**:
  - User registration rate
  - Transaction success rate
  - Feature usage patterns

### 2. Alerting Rules

```yaml
# prometheus-rules.yml
groups:
  - name: application
    rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.1
        for: 2m
        labels:
          severity: warning
        annotations:
          summary: High error rate detected
          description: Error rate is {{ $value }} errors per second

      - alert: SlowResponseTime
        expr: histogram_quantile(0.95, http_request_duration_seconds_bucket) > 2
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: Slow response time detected
          description: 95th percentile response time is {{ $value }} seconds
```

## Performance Considerations

### 1. Sampling Strategies

- **Development**: 100% sampling for debugging
- **Staging**: 50% sampling for testing
- **Production**: 10-20% sampling for cost control

### 2. Batch Processing

```csharp
// Configure batching for better performance
.AddOtlpExporter(otlpOptions =>
{
    otlpOptions.Endpoint = new Uri("http://localhost:4317");
    otlpOptions.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
    {
        MaxQueueSize = 2048,
        ScheduledDelayMilliseconds = 5000,
        MaxExportBatchSize = 512
    };
})
```

### 3. Memory Management

- Use `using` statements for activities
- Dispose of meters and counters properly
- Monitor memory usage of telemetry data

## Troubleshooting

### 1. Common Issues

- **Traces not appearing**: Check sampler configuration and exporter endpoints
- **Metrics missing**: Verify Prometheus scraping configuration
- **High memory usage**: Adjust batch sizes and sampling rates

### 2. Debug Configuration

```csharp
// Enable detailed logging
.AddOtlpExporter(otlpOptions =>
{
    otlpOptions.Endpoint = new Uri("http://localhost:4317");
    otlpOptions.Protocol = OtlpExportProtocol.Grpc;
    otlpOptions.ExportProcessorType = ExportProcessorType.Simple;
})
```

### 3. Health Checks

```csharp
// Add health checks for telemetry endpoints
builder.Services.AddHealthChecks()
    .AddCheck("otel-collector", () =>
    {
        try
        {
            using var client = new HttpClient();
            var response = client.GetAsync("http://localhost:4317/").Result;
            return response.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy();
        }
    });
```

## Conclusion

This comprehensive instrumentation approach provides:

1. **Full visibility** into application performance and behavior
2. **Distributed tracing** for complex microservice architectures
3. **Metrics collection** for monitoring and alerting
4. **Best practices** for production-ready observability
5. **Performance optimization** through sampling and batching
6. **Troubleshooting capabilities** for production issues

By following these patterns, you'll have a robust observability foundation that scales with your application and provides valuable insights into system behavior and performance.
