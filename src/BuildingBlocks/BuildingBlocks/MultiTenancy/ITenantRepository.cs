namespace BuildingBlocks.MultiTenancy;

/// <summary>
/// Repository interface for tenant data access
/// </summary>
public interface ITenantRepository
{
    /// <summary>
    /// Gets a tenant by ID
    /// </summary>
    Task<TenantInfo?> GetByIdAsync(string tenantId);
    
    /// <summary>
    /// Gets a tenant by domain
    /// </summary>
    Task<TenantInfo?> GetByDomainAsync(string domain);
    
    /// <summary>
    /// Gets a tenant by API key
    /// </summary>
    Task<TenantInfo?> GetByApiKeyAsync(string apiKey);
    
    /// <summary>
    /// Creates a new tenant
    /// </summary>
    Task<TenantInfo> CreateAsync(TenantInfo tenant);
    
    /// <summary>
    /// Updates an existing tenant
    /// </summary>
    Task<TenantInfo> UpdateAsync(TenantInfo tenant);
    
    /// <summary>
    /// Deletes a tenant
    /// </summary>
    Task<bool> DeleteAsync(string tenantId);
    
    /// <summary>
    /// Gets all tenants
    /// </summary>
    Task<IEnumerable<TenantInfo>> GetAllAsync();
    
    /// <summary>
    /// Gets tenants by status
    /// </summary>
    Task<IEnumerable<TenantInfo>> GetByStatusAsync(TenantStatus status);
    
    /// <summary>
    /// Gets tenants by tier
    /// </summary>
    Task<IEnumerable<TenantInfo>> GetByTierAsync(TenantTier tier);
    
    /// <summary>
    /// Validates if a user has access to a tenant
    /// </summary>
    Task<bool> ValidateUserAccessAsync(string userId, string tenantId);
    
    /// <summary>
    /// Gets all tenants for a user
    /// </summary>
    Task<IEnumerable<TenantInfo>> GetTenantsForUserAsync(string userId);
    
    /// <summary>
    /// Adds a user to a tenant
    /// </summary>
    Task<bool> AddUserToTenantAsync(string userId, string tenantId, string role = "User");
    
    /// <summary>
    /// Removes a user from a tenant
    /// </summary>
    Task<bool> RemoveUserFromTenantAsync(string userId, string tenantId);
    
    /// <summary>
    /// Gets tenant usage statistics
    /// </summary>
    Task<TenantUsageStats> GetUsageStatsAsync(string tenantId);
    
    /// <summary>
    /// Checks if tenant has exceeded limits
    /// </summary>
    Task<bool> HasExceededLimitsAsync(string tenantId);
}

/// <summary>
/// Tenant usage statistics
/// </summary>
public class TenantUsageStats
{
    public string TenantId { get; set; } = string.Empty;
    public int CurrentUsers { get; set; }
    public int MaxUsers { get; set; }
    public double CurrentStorageGB { get; set; }
    public int MaxStorageGB { get; set; }
    public int CurrentApiCallsThisMonth { get; set; }
    public int MaxApiCallsPerMonth { get; set; }
    public DateTime LastActivity { get; set; }
    public bool IsTrialActive { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public Dictionary<string, int> FeatureUsage { get; set; } = new();
}
