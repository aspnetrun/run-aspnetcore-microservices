using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.MultiTenancy;

/// <summary>
/// Attribute to require tenant context for controllers or actions
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireTenantAttribute : Attribute
{
    /// <summary>
    /// Whether to allow anonymous access when no tenant is specified
    /// </summary>
    public bool AllowAnonymous { get; set; } = false;
    
    /// <summary>
    /// Whether to validate tenant access for the current user
    /// </summary>
    public bool ValidateAccess { get; set; } = true;
    
    /// <summary>
    /// Whether to check tenant limits
    /// </summary>
    public bool CheckLimits { get; set; } = true;
}

/// <summary>
/// Attribute to specify tenant-specific configuration
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TenantSpecificAttribute : Attribute
{
    /// <summary>
    /// The configuration key to use for tenant-specific settings
    /// </summary>
    public string ConfigurationKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to fall back to default configuration if tenant-specific config is not found
    /// </summary>
    public bool FallbackToDefault { get; set; } = true;
}

/// <summary>
/// Attribute to specify tenant isolation requirements
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TenantIsolationAttribute : Attribute
{
    /// <summary>
    /// Whether to enforce strict tenant isolation
    /// </summary>
    public bool StrictIsolation { get; set; } = true;
    
    /// <summary>
    /// Whether to validate tenant boundaries
    /// </summary>
    public bool ValidateBoundaries { get; set; } = true;
}

/// <summary>
/// Attribute to specify tenant feature requirements
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireTenantFeatureAttribute : Attribute
{
    /// <summary>
    /// The feature name required
    /// </summary>
    public string FeatureName { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to check feature limits
    /// </summary>
    public bool CheckLimits { get; set; } = true;
    
    /// <summary>
    /// Whether to allow trial access
    /// </summary>
    public bool AllowTrial { get; set; } = true;
}
