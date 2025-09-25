namespace BuildingBlocks.MultiTenancy;

/// <summary>
/// Represents tenant information for multi-tenant applications
/// </summary>
public class TenantInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public TenantTier Tier { get; set; } = TenantTier.Basic;
    public Dictionary<string, string> Features { get; set; } = new();
    public Dictionary<string, object> Settings { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? AdminEmail { get; set; }
    public string? BillingEmail { get; set; }
    public string? SupportEmail { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string Locale { get; set; } = "en-US";
    public string Currency { get; set; } = "USD";
    public bool IsTrial { get; set; } = false;
    public DateTime? TrialEndsAt { get; set; }
    public int MaxUsers { get; set; } = 10;
    public int MaxStorageGB { get; set; } = 1;
    public int MaxApiCallsPerMonth { get; set; } = 10000;
    public List<string> AllowedIpRanges { get; set; } = new();
    public List<string> BlockedIpRanges { get; set; } = new();
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

/// <summary>
/// Tenant status enumeration
/// </summary>
public enum TenantStatus
{
    Active,
    Suspended,
    Expired,
    Pending,
    Cancelled,
    Deleted
}

/// <summary>
/// Tenant tier enumeration
/// </summary>
public enum TenantTier
{
    Free,
    Basic,
    Professional,
    Enterprise,
    Custom
}

/// <summary>
/// Tenant context for the current request
/// </summary>
public class TenantContext
{
    public string TenantId { get; set; } = string.Empty;
    public TenantInfo? Tenant { get; set; }
    public bool IsResolved { get; set; } = false;
    public string ResolvedBy { get; set; } = string.Empty; // Header, Subdomain, Custom, etc.
    public DateTime ResolvedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Tenant resolution strategy
/// </summary>
public enum TenantResolutionStrategy
{
    Header,           // X-Tenant-Id header
    Subdomain,        // tenant1.yourdomain.com
    Path,             // /tenant1/api/...
    QueryString,      // ?tenant=tenant1
    Custom            // Custom implementation
}
