using Microsoft.Extensions.Options;

namespace BuildingBlocks.MultiTenancy;

/// <summary>
/// Middleware for automatic tenant resolution
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
    private readonly TenantResolutionOptions _options;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger,
        IOptions<TenantResolutionOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolutionService tenantService)
    {
        try
        {
            // Resolve tenant for the current request
            var tenantContext = await tenantService.ResolveTenantAsync(context);
            
            // Store tenant context in HttpContext for access throughout the request
            context.Items["TenantContext"] = tenantContext;
            
            // Validate tenant if required
            if (_options.RequireTenant && !tenantContext.IsResolved)
            {
                if (!_options.AllowAnonymousTenant)
                {
                    _logger.LogWarning("No tenant resolved for request {Path}", context.Request.Path);
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Tenant is required");
                    return;
                }
            }
            
            // Add tenant information to response headers for debugging
            if (tenantContext.IsResolved)
            {
                context.Response.Headers["X-Tenant-Id"] = tenantContext.TenantId;
                context.Response.Headers["X-Tenant-Name"] = tenantContext.Tenant?.Name ?? "Unknown";
                context.Response.Headers["X-Tenant-Tier"] = tenantContext.Tenant?.Tier.ToString() ?? "Unknown";
            }
            
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in tenant resolution middleware");
            
            if (_options.RequireTenant)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Tenant resolution failed");
                return;
            }
            
            await _next(context);
        }
    }
}
