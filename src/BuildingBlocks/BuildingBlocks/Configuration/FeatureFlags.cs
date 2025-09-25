namespace BuildingBlocks.Configuration;

/// <summary>
/// Feature flags for toggling functionality
/// </summary>
public static class FeatureFlags
{
    // Authentication & Authorization
    public const string JwtAuthentication = "JwtAuthentication";
    public const string RoleBasedAuthorization = "RoleBasedAuthorization";
    public const string PermissionBasedAuthorization = "PermissionBasedAuthorization";
    
    // Auditing
    public const string AuditLogging = "AuditLogging";
    public const string RequestAuditing = "RequestAuditing";
    public const string ResponseAuditing = "ResponseAuditing";
    
    // Caching
    public const string RedisCaching = "RedisCaching";
    public const string MemoryCaching = "MemoryCaching";
    
    // Resilience
    public const string CircuitBreaker = "CircuitBreaker";
    public const string RetryPolicy = "RetryPolicy";
    public const string BulkheadPolicy = "BulkheadPolicy";
    
    // Monitoring
    public const string OpenTelemetry = "OpenTelemetry";
    public const string HealthChecks = "HealthChecks";
    public const string Metrics = "Metrics";
    
    // Business Features
    public const string OrderFulfillment = "OrderFulfillment";
    public const string DiscountCalculation = "DiscountCalculation";
    public const string InventoryManagement = "InventoryManagement";
    public const string PaymentProcessing = "PaymentProcessing";
    
    // Development Features
    public const string SwaggerDocumentation = "SwaggerDocumentation";
    public const string DetailedErrorMessages = "DetailedErrorMessages";
    public const string PerformanceProfiling = "PerformanceProfiling";
}

/// <summary>
/// Feature flag service interface
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Checks if a feature is enabled
    /// </summary>
    bool IsEnabled(string featureName);
    
    /// <summary>
    /// Checks if a feature is enabled for a specific user
    /// </summary>
    bool IsEnabledForUser(string featureName, string userId);
    
    /// <summary>
    /// Gets all enabled features
    /// </summary>
    IEnumerable<string> GetEnabledFeatures();
    
    /// <summary>
    /// Gets all disabled features
    /// </summary>
    IEnumerable<string> GetDisabledFeatures();
    
    /// <summary>
    /// Enables a feature
    /// </summary>
    Task EnableFeatureAsync(string featureName);
    
    /// <summary>
    /// Disables a feature
    /// </summary>
    Task DisableFeatureAsync(string featureName);
}

/// <summary>
/// Default feature flag service implementation
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, bool> _featureFlags;

    public FeatureFlagService(IConfiguration configuration)
    {
        _configuration = configuration;
        _featureFlags = new Dictionary<string, bool>();
        LoadFeatureFlags();
    }

    public bool IsEnabled(string featureName)
    {
        return _featureFlags.TryGetValue(featureName, out var enabled) && enabled;
    }

    public bool IsEnabledForUser(string featureName, string userId)
    {
        // Check if feature is globally enabled
        if (!IsEnabled(featureName))
            return false;

        // Check user-specific overrides
        var userOverride = _configuration[$"FeatureFlags:Users:{userId}:{featureName}"];
        if (!string.IsNullOrEmpty(userOverride))
            return bool.Parse(userOverride);

        return true;
    }

    public IEnumerable<string> GetEnabledFeatures()
    {
        return _featureFlags.Where(kvp => kvp.Value).Select(kvp => kvp.Key);
    }

    public IEnumerable<string> GetDisabledFeatures()
    {
        return _featureFlags.Where(kvp => !kvp.Value).Select(kvp => kvp.Key);
    }

    public async Task EnableFeatureAsync(string featureName)
    {
        _featureFlags[featureName] = true;
        await Task.CompletedTask;
    }

    public async Task DisableFeatureAsync(string featureName)
    {
        _featureFlags[featureName] = false;
        await Task.CompletedTask;
    }

    private void LoadFeatureFlags()
    {
        var featureFlagsSection = _configuration.GetSection("FeatureFlags");
        
        // Load all feature flags from configuration
        foreach (var feature in typeof(FeatureFlags).GetFields())
        {
            var featureName = feature.GetValue(null)?.ToString();
            if (!string.IsNullOrEmpty(featureName))
            {
                var isEnabled = featureFlagsSection.GetValue<bool>(featureName);
                _featureFlags[featureName] = isEnabled;
            }
        }
    }
}
