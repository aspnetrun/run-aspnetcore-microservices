# Observability Setup Guide

This project has been configured with comprehensive observability using OpenTelemetry, Jaeger, and Prometheus.

## Architecture Overview

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────┐
│   Microservices │───▶│  OTEL Collector  │───▶│   Jaeger    │
│                 │    │                  │    │  (Tracing)  │
└─────────────────┘    └──────────────────┘    └─────────────┘
                                │
                                ▼
                       ┌──────────────────┐
                       │    Prometheus    │
                       │   (Metrics)      │
                       └──────────────────┘
```

## Services and Ports

### Observability Services
- **OpenTelemetry Collector**: `http://localhost:4317` (gRPC), `http://localhost:4318` (HTTP)
- **Jaeger UI**: `http://localhost:16686`
- **Prometheus**: `http://localhost:9090`

### Application Services
- **Catalog API**: `http://localhost:6000`
- **Basket API**: `http://localhost:6001`
- **Discount gRPC**: `http://localhost:6002`
- **Ordering API**: `http://localhost:6003`
- **YARP Gateway**: `http://localhost:6004`
- **Shopping Web**: `http://localhost:6005`

## Getting Started

### 1. Start the Services

```bash
cd src
docker-compose up -d
```

### 2. Access Observability Tools

#### Jaeger (Distributed Tracing)
- URL: `http://localhost:16686`
- Use to trace requests across microservices
- View service dependencies and performance bottlenecks

#### Prometheus (Metrics)
- URL: `http://localhost:9090`
- View collected metrics and create queries
- Access metrics endpoints at `/metrics` on each service

#### OpenTelemetry Collector
- Collects telemetry data from all services
- Exports to Jaeger and Prometheus
- Health check: `http://localhost:8889/`

### 3. View Service Metrics

Each service exposes Prometheus metrics at `/metrics`:

```bash
# Catalog API metrics
curl http://localhost:6000/metrics

# Basket API metrics  
curl http://localhost:6001/metrics

# Ordering API metrics
curl http://localhost:6003/metrics

# YARP Gateway metrics
curl http://localhost:6004/metrics

# Shopping Web metrics
curl http://localhost:6005/metrics
```

## Available Metrics

### HTTP Metrics
- `http_requests_total` - Total HTTP requests
- `http_request_duration_seconds` - Request duration histogram
- `http_requests_active` - Active requests

### Runtime Metrics
- `process_cpu_seconds_total` - CPU usage
- `process_memory_bytes` - Memory usage
- `dotnet_total_memory_bytes` - .NET memory usage

### Custom Business Metrics
- `products_requested_total` - Products requested counter
- `products_request_duration_seconds` - Product request duration
- `rabbitmq_messages_processed_total` - RabbitMQ messages processed
- `rabbitmq_message_processing_duration_seconds` - Message processing duration

## Tracing

### View Traces in Jaeger
1. Go to `http://localhost:16686`
2. Select a service from the dropdown
3. Click "Find Traces"
4. View trace details and spans

### Trace Propagation
Traces automatically propagate across services through:
- HTTP headers
- gRPC metadata
- Message broker headers

## Health Checks

### Service Health
Each service includes health checks at `/health`:
- Database connectivity
- OpenTelemetry collector connectivity
- Service-specific health indicators

### Container Health
All containers include health checks:
- Database services check connectivity
- Redis checks ping response
- RabbitMQ checks management API
- Observability services check endpoints

## Configuration

### Environment Variables
- `OpenTelemetry__Endpoint`: OpenTelemetry collector endpoint
- `ASPNETCORE_ENVIRONMENT`: Environment name

### Sampling
- **Development**: 100% sampling (all traces collected)
- **Production**: 100% sampling (configurable via code)

### Resource Attributes
- `service.name`: Service identifier
- `service.version`: Service version
- `environment`: Environment name
- `deployment.region`: Deployment region
- `service.instance.id`: Machine identifier

## Custom Instrumentation

### Adding Custom Metrics

```csharp
public class MyController : ControllerBase
{
    private readonly IMeterFactory _meterFactory;
    
    public MyController(IMeterFactory meterFactory)
    {
        _meterFactory = meterFactory;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        var meter = _meterFactory.Create("MyController");
        var counter = meter.CreateCounter<long>("my_operation_total");
        
        counter.Add(1);
        return Ok();
    }
}
```

### Adding Custom Tracing

```csharp
public class MyService
{
    private readonly ActivitySource _activitySource;
    
    public MyService(ActivitySource activitySource)
    {
        _activitySource = activitySource;
    }
    
    public async Task DoWorkAsync()
    {
        using var activity = _activitySource.StartActivity("MyService.DoWork");
        activity?.SetTag("operation.type", "async");
        
        try
        {
            // Your work here
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
```

## Troubleshooting

### Common Issues

#### Traces Not Appearing
1. Check OpenTelemetry collector is running: `docker ps`
2. Verify collector health: `curl http://localhost:8889/`
3. Check service configuration has correct endpoint

#### Metrics Missing
1. Verify Prometheus configuration in `prometheus.yml`
2. Check service metrics endpoints: `curl /metrics`
3. Ensure OpenTelemetry is properly configured

#### High Memory Usage
1. Check sampling configuration
2. Verify batch processing settings
3. Monitor collector resource usage

### Debug Mode
Enable detailed logging by setting:
```bash
export OTEL_LOG_LEVEL=DEBUG
```

### Health Check Commands
```bash
# Check all container health
docker-compose ps

# Check specific service logs
docker-compose logs otel-collector
docker-compose logs jaeger
docker-compose logs prometheus
```

## Performance Considerations

### Sampling Strategy
- Current: 100% sampling for all environments
- Production: Consider reducing to 10-20% for cost control

### Batch Processing
- OpenTelemetry collector batches data for efficiency
- Configurable batch size and timeout in `otel-collector-config.yaml`

### Memory Management
- Activities are automatically disposed with `using` statements
- Meters and counters are long-lived and shared

## Next Steps

### Advanced Features
1. **Grafana Dashboards**: Create custom dashboards for metrics
2. **Alerting**: Configure Prometheus alerting rules
3. **Log Aggregation**: Add centralized logging (ELK stack)
4. **Service Mesh**: Integrate with Istio for advanced tracing

### Production Deployment
1. **Persistent Storage**: Configure persistent volumes for Prometheus
2. **High Availability**: Deploy multiple collector instances
3. **Security**: Enable TLS and authentication
4. **Monitoring**: Monitor the monitoring stack itself

## Support

For issues or questions:
1. Check container logs: `docker-compose logs <service>`
2. Verify configuration files
3. Test individual endpoints
4. Check network connectivity between services
