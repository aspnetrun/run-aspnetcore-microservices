using BuildingBlocks.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Examples;

/// <summary>
/// Example configuration class with validation
/// </summary>
[ConfigurationValidation(SectionName = "Jwt")]
public class JwtConfiguration : BaseConfiguration
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationHours { get; set; } = 24;

    public override bool IsValid()
    {
        return !string.IsNullOrEmpty(SecretKey) &&
               !string.IsNullOrEmpty(Issuer) &&
               !string.IsNullOrEmpty(Audience) &&
               ExpirationHours > 0;
    }

    public override IEnumerable<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrEmpty(SecretKey))
            errors.Add("JWT SecretKey is required");
        
        if (string.IsNullOrEmpty(Issuer))
            errors.Add("JWT Issuer is required");
        
        if (string.IsNullOrEmpty(Audience))
            errors.Add("JWT Audience is required");
        
        if (ExpirationHours <= 0)
            errors.Add("JWT ExpirationHours must be greater than 0");
        
        return errors;
    }
}

/// <summary>
/// Example configuration class for database settings
/// </summary>
[ConfigurationValidation(SectionName = "Database")]
public class DatabaseConfiguration : BaseConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetryCount { get; set; } = 3;
    public int CommandTimeout { get; set; } = 30;

    public override bool IsValid()
    {
        return !string.IsNullOrEmpty(ConnectionString) &&
               MaxRetryCount > 0 &&
               CommandTimeout > 0;
    }

    public override IEnumerable<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrEmpty(ConnectionString))
            errors.Add("Database ConnectionString is required");
        
        if (MaxRetryCount <= 0)
            errors.Add("Database MaxRetryCount must be greater than 0");
        
        if (CommandTimeout <= 0)
            errors.Add("Database CommandTimeout must be greater than 0");
        
        return errors;
    }
}

/// <summary>
/// Example controller showing configuration building blocks usage
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConfigurationExampleController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly IConfigurationValidationService _configValidationService;
    private readonly JwtConfiguration _jwtConfig;
    private readonly DatabaseConfiguration _dbConfig;

    public ConfigurationExampleController(
        IFeatureFlagService featureFlagService,
        IConfigurationValidationService configValidationService,
        IOptions<JwtConfiguration> jwtConfig,
        IOptions<DatabaseConfiguration> dbConfig)
    {
        _featureFlagService = featureFlagService;
        _configValidationService = configValidationService;
        _jwtConfig = jwtConfig.Value;
        _dbConfig = dbConfig.Value;
    }

    /// <summary>
    /// Get current configuration status
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetConfigurationStatus()
    {
        var validationResult = await _configValidationService.ValidateAllAsync();
        
        return Ok(new
        {
            jwtConfiguration = new
            {
                isValid = _jwtConfig.IsValid(),
                errors = _jwtConfig.GetValidationErrors(),
                issuer = _jwtConfig.Issuer,
                audience = _jwtConfig.Audience,
                expirationHours = _jwtConfig.ExpirationHours
            },
            databaseConfiguration = new
            {
                isValid = _dbConfig.IsValid(),
                errors = _dbConfig.GetValidationErrors(),
                maxRetryCount = _dbConfig.MaxRetryCount,
                commandTimeout = _dbConfig.CommandTimeout
            },
            overallValidation = new
            {
                isValid = validationResult.IsValid,
                errors = validationResult.Errors,
                warnings = validationResult.Warnings
            }
        });
    }

    /// <summary>
    /// Get feature flags status
    /// </summary>
    [HttpGet("features")]
    public IActionResult GetFeatureFlags()
    {
        return Ok(new
        {
            enabledFeatures = _featureFlagService.GetEnabledFeatures(),
            disabledFeatures = _featureFlagService.GetDisabledFeatures(),
            jwtEnabled = _featureFlagService.IsEnabled(FeatureFlags.JwtAuthentication),
            auditEnabled = _featureFlagService.IsEnabled(FeatureFlags.AuditLogging),
            openTelemetryEnabled = _featureFlagService.IsEnabled(FeatureFlags.OpenTelemetry)
        });
    }

    /// <summary>
    /// Check if a specific feature is enabled for a user
    /// </summary>
    [HttpGet("features/{featureName}/user/{userId}")]
    public IActionResult CheckFeatureForUser(string featureName, string userId)
    {
        var isEnabled = _featureFlagService.IsEnabledForUser(featureName, userId);
        
        return Ok(new
        {
            featureName,
            userId,
            isEnabled,
            globallyEnabled = _featureFlagService.IsEnabled(featureName)
        });
    }

    /// <summary>
    /// Validate a specific configuration section
    /// </summary>
    [HttpGet("validate/{sectionName}")]
    public async Task<IActionResult> ValidateConfigurationSection(string sectionName)
    {
        var result = await _configValidationService.ValidateSectionAsync(sectionName);
        
        return Ok(new
        {
            sectionName,
            isValid = result.IsValid,
            errors = result.Errors,
            warnings = result.Warnings
        });
    }
}
