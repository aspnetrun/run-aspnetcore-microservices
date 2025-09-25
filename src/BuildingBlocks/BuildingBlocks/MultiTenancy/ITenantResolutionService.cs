namespace BuildingBlocks.MultiTenancy;

/// <summary>
/// Service for resolving tenant information from various sources
/// </summary>
public interface ITenantResolutionService
{
    /// <summary>
    /// Resolves the current tenant from the HTTP context
    /// </summary>
    Task<TenantContext> ResolveTenantAsync(HttpContext httpContext);
    
    /// <summary>
    /// Resolves tenant by ID
    /// </summary>
    Task<TenantInfo?> ResolveTenantByIdAsync(string tenantId);
    
    /// <summary>
    /// Resolves tenant by domain
    /// </summary>
    Task<TenantInfo?> ResolveTenantByDomainAsync(string domain);
    
    /// <summary>
    /// Resolves tenant by API key
    /// </summary>
    Task<TenantInfo?> ResolveTenantByApiKeyAsync(string apiKey);
    
    /// <summary>
    /// Gets the current tenant context
    /// </summary>
    TenantContext GetCurrentTenant();
    
    /// <summary>
    /// Sets the current tenant context
    /// </summary>
    void SetCurrentTenant(TenantContext context);
    
    /// <summary>
    /// Clears the current tenant context
    /// </summary>
    void ClearCurrentTenant();
    
    /// <summary>
    /// Validates if the current user has access to the tenant
    /// </summary>
    Task<bool> ValidateTenantAccessAsync(string userId, string tenantId);
    
    /// <summary>
    /// Gets all tenants for a user
    /// </summary>
    Task<IEnumerable<TenantInfo>> GetTenantsForUserAsync(string userId);
}

/// <summary>
/// Tenant resolution options
/// </summary>
public class TenantResolutionOptions
{
    public TenantResolutionStrategy DefaultStrategy { get; set; } = TenantResolutionStrategy.Header;
    public string HeaderName { get; set; } = "X-Tenant-Id";
    public string QueryStringName { get; set; } = "tenant";
    public string PathSegmentName { get; set; } = "tenant";
    public string SubdomainPrefix { get; set; } = "";
    public bool RequireTenant { get; set; } = true;
    public bool CacheTenantInfo { get; set; } = true;
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(30);
    public string DefaultTenantId { get; set; } = "default";
    public bool AllowAnonymousTenant { get; set; } = false;
}
