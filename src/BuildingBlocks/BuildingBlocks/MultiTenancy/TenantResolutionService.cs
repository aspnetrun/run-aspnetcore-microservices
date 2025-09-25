using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.MultiTenancy;

/// <summary>
/// Default implementation of tenant resolution service
/// </summary>
public class TenantResolutionService : ITenantResolutionService
{
    private readonly ILogger<TenantResolutionService> _logger;
    private readonly IMemoryCache _cache;
    private readonly TenantResolutionOptions _options;
    private readonly AsyncLocal<TenantContext> _currentTenant = new();
    private readonly ITenantRepository _tenantRepository;

    public TenantResolutionService(
        ILogger<TenantResolutionService> logger,
        IMemoryCache cache,
        IOptions<TenantResolutionOptions> options,
        ITenantRepository tenantRepository)
    {
        _logger = logger;
        _cache = cache;
        _options = options.Value;
        _tenantRepository = tenantRepository;
    }

    public async Task<TenantContext> ResolveTenantAsync(HttpContext httpContext)
    {
        var context = new TenantContext();
        
        try
        {
            // Try different resolution strategies in order of preference
            context = await ResolveByHeaderAsync(httpContext) ??
                     await ResolveBySubdomainAsync(httpContext) ??
                     await ResolveByPathAsync(httpContext) ??
                     await ResolveByQueryStringAsync(httpContext) ??
                     await ResolveByCustomStrategyAsync(httpContext) ??
                     await ResolveDefaultTenantAsync();

            if (context != null && !string.IsNullOrEmpty(context.TenantId))
            {
                // Resolve tenant information
                var tenantInfo = await ResolveTenantInfoAsync(context.TenantId);
                if (tenantInfo != null)
                {
                    context.Tenant = tenantInfo;
                    context.IsResolved = true;
                    
                    // Cache tenant info if enabled
                    if (_options.CacheTenantInfo)
                    {
                        var cacheKey = $"tenant:{context.TenantId}";
                        _cache.Set(cacheKey, tenantInfo, _options.CacheExpiration);
                    }
                }
                else
                {
                    _logger.LogWarning("Tenant {TenantId} not found", context.TenantId);
                }
            }

            // Set current tenant context
            SetCurrentTenant(context);
            
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant");
            return new TenantContext { IsResolved = false };
        }
    }

    public async Task<TenantInfo?> ResolveTenantByIdAsync(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            return null;

        // Check cache first
        if (_options.CacheTenantInfo)
        {
            var cacheKey = $"tenant:{tenantId}";
            if (_cache.TryGetValue(cacheKey, out TenantInfo? cachedTenant))
                return cachedTenant;
        }

        // Resolve from repository
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        
        // Cache if found
        if (tenant != null && _options.CacheTenantInfo)
        {
            var cacheKey = $"tenant:{tenantId}";
            _cache.Set(cacheKey, tenant, _options.CacheExpiration);
        }

        return tenant;
    }

    public async Task<TenantInfo?> ResolveTenantByDomainAsync(string domain)
    {
        if (string.IsNullOrEmpty(domain))
            return null;

        var cacheKey = $"tenant:domain:{domain}";
        
        if (_options.CacheTenantInfo && _cache.TryGetValue(cacheKey, out TenantInfo? cachedTenant))
            return cachedTenant;

        var tenant = await _tenantRepository.GetByDomainAsync(domain);
        
        if (tenant != null && _options.CacheTenantInfo)
            _cache.Set(cacheKey, tenant, _options.CacheExpiration);

        return tenant;
    }

    public async Task<TenantInfo?> ResolveTenantByApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            return null;

        var cacheKey = $"tenant:apikey:{apiKey}";
        
        if (_options.CacheTenantInfo && _cache.TryGetValue(cacheKey, out TenantInfo? cachedTenant))
            return cachedTenant;

        var tenant = await _tenantRepository.GetByApiKeyAsync(apiKey);
        
        if (tenant != null && _options.CacheTenantInfo)
            _cache.Set(cacheKey, tenant, _options.CacheExpiration);

        return tenant;
    }

    public TenantContext GetCurrentTenant()
    {
        return _currentTenant.Value ?? new TenantContext();
    }

    public void SetCurrentTenant(TenantContext context)
    {
        _currentTenant.Value = context;
    }

    public void ClearCurrentTenant()
    {
        _currentTenant.Value = null;
    }

    public async Task<bool> ValidateTenantAccessAsync(string userId, string tenantId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
            return false;

        return await _tenantRepository.ValidateUserAccessAsync(userId, tenantId);
    }

    public async Task<IEnumerable<TenantInfo>> GetTenantsForUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return Enumerable.Empty<TenantInfo>();

        return await _tenantRepository.GetTenantsForUserAsync(userId);
    }

    private async Task<TenantContext?> ResolveByHeaderAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue(_options.HeaderName, out var headerValue))
        {
            var tenantId = headerValue.FirstOrDefault();
            if (!string.IsNullOrEmpty(tenantId))
            {
                return new TenantContext
                {
                    TenantId = tenantId,
                    ResolvedBy = "Header",
                    ResolvedAt = DateTime.UtcNow
                };
            }
        }
        return null;
    }

    private async Task<TenantContext?> ResolveBySubdomainAsync(HttpContext httpContext)
    {
        var host = httpContext.Request.Host.Host;
        if (host.Contains('.'))
        {
            var subdomain = host.Split('.')[0];
            if (!string.IsNullOrEmpty(subdomain) && subdomain != "www" && subdomain != "api")
            {
                var tenant = await ResolveTenantByDomainAsync(subdomain);
                if (tenant != null)
                {
                    return new TenantContext
                    {
                        TenantId = tenant.Id,
                        ResolvedBy = "Subdomain",
                        ResolvedAt = DateTime.UtcNow
                    };
                }
            }
        }
        return null;
    }

    private async Task<TenantContext?> ResolveByPathAsync(HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value;
        if (!string.IsNullOrEmpty(path))
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0 && segments[0] == _options.PathSegmentName && segments.Length > 1)
            {
                var tenantId = segments[1];
                if (!string.IsNullOrEmpty(tenantId))
                {
                    return new TenantContext
                    {
                        TenantId = tenantId,
                        ResolvedBy = "Path",
                        ResolvedAt = DateTime.UtcNow
                    };
                }
            }
        }
        return null;
    }

    private async Task<TenantContext?> ResolveByQueryStringAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Query.TryGetValue(_options.QueryStringName, out var queryValue))
        {
            var tenantId = queryValue.FirstOrDefault();
            if (!string.IsNullOrEmpty(tenantId))
            {
                return new TenantContext
                {
                    TenantId = tenantId,
                    ResolvedBy = "QueryString",
                    ResolvedAt = DateTime.UtcNow
                };
            }
        }
        return null;
    }

    private async Task<TenantContext?> ResolveByCustomStrategyAsync(HttpContext httpContext)
    {
        // Custom strategy implementation can be injected
        // For now, return null
        return null;
    }

    private async Task<TenantContext> ResolveDefaultTenantAsync()
    {
        if (!_options.RequireTenant)
        {
            return new TenantContext
            {
                TenantId = _options.DefaultTenantId,
                ResolvedBy = "Default",
                ResolvedAt = DateTime.UtcNow
            };
        }

        return new TenantContext
        {
            TenantId = string.Empty,
            ResolvedBy = "None",
            ResolvedAt = DateTime.UtcNow
        };
    }

    private async Task<TenantInfo?> ResolveTenantInfoAsync(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            return null;

        return await ResolveTenantByIdAsync(tenantId);
    }
}
