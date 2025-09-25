using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.MultiTenancy;

/// <summary>
/// Extension methods for multi-tenancy services
/// </summary>
public static class MultiTenancyExtensions
{
    /// <summary>
    /// Adds multi-tenancy services to the service collection
    /// </summary>
    public static IServiceCollection AddMultiTenancy(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<TenantResolutionOptions>? configureOptions = null)
    {
        // Configure options
        var options = new TenantResolutionOptions();
        configureOptions?.Invoke(options);
        
        services.Configure<TenantResolutionOptions>(config =>
        {
            config.DefaultStrategy = options.DefaultStrategy;
            config.HeaderName = options.HeaderName;
            config.QueryStringName = options.QueryStringName;
            config.PathSegmentName = options.PathSegmentName;
            config.SubdomainPrefix = options.SubdomainPrefix;
            config.RequireTenant = options.RequireTenant;
            config.CacheTenantInfo = options.CacheTenantInfo;
            config.CacheExpiration = options.CacheExpiration;
            config.DefaultTenantId = options.DefaultTenantId;
            config.AllowAnonymousTenant = options.AllowAnonymousTenant;
        });

        // Register services
        services.AddMemoryCache();
        services.AddScoped<ITenantResolutionService, TenantResolutionService>();
        services.AddScoped<ITenantRepository, InMemoryTenantRepository>(); // Default implementation
        
        // Register tenant context accessor
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        
        return services;
    }

    /// <summary>
    /// Adds multi-tenancy with custom tenant repository
    /// </summary>
    public static IServiceCollection AddMultiTenancy<TRepository>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<TenantResolutionOptions>? configureOptions = null)
        where TRepository : class, ITenantRepository
    {
        services.AddMultiTenancy(configuration, configureOptions);
        services.AddScoped<ITenantRepository, TRepository>();
        return services;
    }

    /// <summary>
    /// Adds multi-tenancy middleware
    /// </summary>
    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantResolutionMiddleware>();
        return app;
    }
}

/// <summary>
/// Tenant context accessor for dependency injection
/// </summary>
public interface ITenantContextAccessor
{
    TenantContext CurrentTenant { get; }
    void SetCurrentTenant(TenantContext context);
}

/// <summary>
/// Default tenant context accessor implementation
/// </summary>
public class TenantContextAccessor : ITenantContextAccessor
{
    private readonly AsyncLocal<TenantContext> _currentTenant = new();

    public TenantContext CurrentTenant
    {
        get => _currentTenant.Value ?? new TenantContext();
        set => _currentTenant.Value = value;
    }

    public void SetCurrentTenant(TenantContext context)
    {
        _currentTenant.Value = context;
    }
}

/// <summary>
/// In-memory tenant repository for development/testing
/// </summary>
public class InMemoryTenantRepository : ITenantRepository
{
    private readonly Dictionary<string, TenantInfo> _tenants = new();
    private readonly Dictionary<string, List<string>> _userTenants = new();

    public InMemoryTenantRepository()
    {
        // Add some sample tenants
        var defaultTenant = new TenantInfo
        {
            Id = "default",
            Name = "Default Tenant",
            Domain = "default",
            Status = TenantStatus.Active,
            Tier = TenantTier.Basic,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };
        
        _tenants[defaultTenant.Id] = defaultTenant;
    }

    public Task<TenantInfo?> GetByIdAsync(string tenantId)
    {
        _tenants.TryGetValue(tenantId, out var tenant);
        return Task.FromResult(tenant);
    }

    public Task<TenantInfo?> GetByDomainAsync(string domain)
    {
        var tenant = _tenants.Values.FirstOrDefault(t => t.Domain == domain);
        return Task.FromResult(tenant);
    }

    public Task<TenantInfo?> GetByApiKeyAsync(string apiKey)
    {
        var tenant = _tenants.Values.FirstOrDefault(t => t.ApiKey == apiKey);
        return Task.FromResult(tenant);
    }

    public Task<TenantInfo> CreateAsync(TenantInfo tenant)
    {
        tenant.Id = Guid.NewGuid().ToString();
        _tenants[tenant.Id] = tenant;
        return Task.FromResult(tenant);
    }

    public Task<TenantInfo> UpdateAsync(TenantInfo tenant)
    {
        if (_tenants.ContainsKey(tenant.Id))
        {
            _tenants[tenant.Id] = tenant;
        }
        return Task.FromResult(tenant);
    }

    public Task<bool> DeleteAsync(string tenantId)
    {
        var removed = _tenants.Remove(tenantId);
        return Task.FromResult(removed);
    }

    public Task<IEnumerable<TenantInfo>> GetAllAsync()
    {
        return Task.FromResult(_tenants.Values.AsEnumerable());
    }

    public Task<IEnumerable<TenantInfo>> GetByStatusAsync(TenantStatus status)
    {
        var tenants = _tenants.Values.Where(t => t.Status == status);
        return Task.FromResult(tenants);
    }

    public Task<IEnumerable<TenantInfo>> GetByTierAsync(TenantTier tier)
    {
        var tenants = _tenants.Values.Where(t => t.Tier == tier);
        return Task.FromResult(tenants);
    }

    public Task<bool> ValidateUserAccessAsync(string userId, string tenantId)
    {
        if (_userTenants.TryGetValue(userId, out var userTenants))
        {
            return Task.FromResult(userTenants.Contains(tenantId));
        }
        return Task.FromResult(false);
    }

    public Task<IEnumerable<TenantInfo>> GetTenantsForUserAsync(string userId)
    {
        if (_userTenants.TryGetValue(userId, out var userTenants))
        {
            var tenants = userTenants.Select(id => _tenants.GetValueOrDefault(id)).Where(t => t != null);
            return Task.FromResult(tenants!);
        }
        return Task.FromResult(Enumerable.Empty<TenantInfo>());
    }

    public Task<bool> AddUserToTenantAsync(string userId, string tenantId, string role = "User")
    {
        if (!_userTenants.ContainsKey(userId))
        {
            _userTenants[userId] = new List<string>();
        }
        
        if (!_userTenants[userId].Contains(tenantId))
        {
            _userTenants[userId].Add(tenantId);
        }
        
        return Task.FromResult(true);
    }

    public Task<bool> RemoveUserFromTenantAsync(string userId, string tenantId)
    {
        if (_userTenants.TryGetValue(userId, out var userTenants))
        {
            return Task.FromResult(userTenants.Remove(tenantId));
        }
        return Task.FromResult(false);
    }

    public Task<TenantUsageStats> GetUsageStatsAsync(string tenantId)
    {
        var tenant = _tenants.GetValueOrDefault(tenantId);
        if (tenant == null)
        {
            return Task.FromResult(new TenantUsageStats { TenantId = tenantId });
        }

        var stats = new TenantUsageStats
        {
            TenantId = tenantId,
            MaxUsers = tenant.MaxUsers,
            MaxStorageGB = tenant.MaxStorageGB,
            MaxApiCallsPerMonth = tenant.MaxApiCallsPerMonth,
            IsTrialActive = tenant.IsTrial,
            TrialEndsAt = tenant.TrialEndsAt
        };

        return Task.FromResult(stats);
    }

    public Task<bool> HasExceededLimitsAsync(string tenantId)
    {
        // Simple implementation - always return false for demo
        return Task.FromResult(false);
    }
}
