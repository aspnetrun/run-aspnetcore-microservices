using BuildingBlocks.MultiTenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Examples;

/// <summary>
/// Example controller showing multi-tenancy building blocks usage
/// </summary>
[ApiController]
[Route("api/[controller]")]
[RequireTenant]
public class MultiTenancyExampleController : ControllerBase
{
    private readonly ITenantResolutionService _tenantService;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly ITenantRepository _tenantRepository;
    private readonly IFeatureFlagService _featureFlags;

    public MultiTenancyExampleController(
        ITenantResolutionService tenantService,
        ITenantContextAccessor tenantContextAccessor,
        ITenantRepository tenantRepository,
        IFeatureFlagService featureFlags)
    {
        _tenantService = tenantService;
        _tenantContextAccessor = tenantContextAccessor;
        _tenantRepository = tenantRepository;
        _featureFlags = featureFlags;
    }

    /// <summary>
    /// Get current tenant information
    /// </summary>
    [HttpGet("current")]
    public IActionResult GetCurrentTenant()
    {
        var currentTenant = _tenantContextAccessor.CurrentTenant;
        
        return Ok(new
        {
            tenantId = currentTenant.TenantId,
            tenantName = currentTenant.Tenant?.Name,
            tier = currentTenant.Tenant?.Tier,
            status = currentTenant.Tenant?.Status,
            resolvedBy = currentTenant.ResolvedBy,
            resolvedAt = currentTenant.ResolvedAt,
            isResolved = currentTenant.IsResolved
        });
    }

    /// <summary>
    /// Get tenant by ID
    /// </summary>
    [HttpGet("tenants/{tenantId}")]
    [RequireTenantFeature(FeatureFlags.AdminAccess)]
    public async Task<IActionResult> GetTenant(string tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        
        if (tenant == null)
            return NotFound($"Tenant {tenantId} not found");

        return Ok(tenant);
    }

    /// <summary>
    /// Create a new tenant
    /// </summary>
    [HttpPost("tenants")]
    [RequireTenantFeature(FeatureFlags.AdminAccess)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
    {
        var tenant = new TenantInfo
        {
            Name = request.Name,
            Domain = request.Domain,
            Tier = request.Tier,
            Status = TenantStatus.Pending,
            CreatedBy = User.Identity?.Name ?? "system",
            MaxUsers = request.MaxUsers,
            MaxStorageGB = request.MaxStorageGB,
            MaxApiCallsPerMonth = request.MaxApiCallsPerMonth
        };

        var createdTenant = await _tenantRepository.CreateAsync(tenant);
        
        return CreatedAtAction(nameof(GetTenant), new { tenantId = createdTenant.Id }, createdTenant);
    }

    /// <summary>
    /// Get tenant usage statistics
    /// </summary>
    [HttpGet("tenants/{tenantId}/usage")]
    [RequireTenantFeature(FeatureFlags.AdminAccess)]
    public async Task<IActionResult> GetTenantUsage(string tenantId)
    {
        var usageStats = await _tenantRepository.GetUsageStatsAsync(tenantId);
        var hasExceededLimits = await _tenantRepository.HasExceededLimitsAsync(tenantId);
        
        return Ok(new
        {
            usage = usageStats,
            hasExceededLimits,
            warnings = hasExceededLimits ? new[] { "Tenant has exceeded usage limits" } : Array.Empty<string>()
        });
    }

    /// <summary>
    /// Get all tenants for current user
    /// </summary>
    [HttpGet("my-tenants")]
    public async Task<IActionResult> GetMyTenants()
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var tenants = await _tenantService.GetTenantsForUserAsync(userId);
        
        return Ok(tenants);
    }

    /// <summary>
    /// Check if user has access to current tenant
    /// </summary>
    [HttpGet("access-check")]
    public async Task<IActionResult> CheckAccess()
    {
        var userId = User.Identity?.Name;
        var currentTenant = _tenantContextAccessor.CurrentTenant;
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(currentTenant.TenantId))
            return Unauthorized();

        var hasAccess = await _tenantService.ValidateTenantAccessAsync(userId, currentTenant.TenantId);
        
        return Ok(new
        {
            userId,
            tenantId = currentTenant.TenantId,
            hasAccess,
            message = hasAccess ? "Access granted" : "Access denied"
        });
    }

    /// <summary>
    /// Get tenant-specific configuration
    /// </summary>
    [HttpGet("config")]
    [TenantSpecific(ConfigurationKey = "TenantSettings")]
    public IActionResult GetTenantConfig()
    {
        var currentTenant = _tenantContextAccessor.CurrentTenant;
        
        // In a real implementation, you would load tenant-specific configuration
        var config = new
        {
            tenantId = currentTenant.TenantId,
            features = currentTenant.Tenant?.Features ?? new Dictionary<string, string>(),
            settings = currentTenant.Tenant?.Settings ?? new Dictionary<string, object>(),
            limits = new
            {
                maxUsers = currentTenant.Tenant?.MaxUsers,
                maxStorageGB = currentTenant.Tenant?.MaxStorageGB,
                maxApiCallsPerMonth = currentTenant.Tenant?.MaxApiCallsPerMonth
            }
        };
        
        return Ok(config);
    }

    /// <summary>
    /// Get tenant isolation status
    /// </summary>
    [HttpGet("isolation")]
    [TenantIsolation]
    public IActionResult GetIsolationStatus()
    {
        var currentTenant = _tenantContextAccessor.CurrentTenant;
        
        return Ok(new
        {
            tenantId = currentTenant.TenantId,
            isIsolated = true,
            isolationLevel = "Strict",
            boundaries = new
            {
                dataIsolation = true,
                userIsolation = true,
                apiIsolation = true
            }
        });
    }
}

public class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public TenantTier Tier { get; set; } = TenantTier.Basic;
    public int MaxUsers { get; set; } = 10;
    public int MaxStorageGB { get; set; } = 1;
    public int MaxApiCallsPerMonth { get; set; } = 10000;
}
