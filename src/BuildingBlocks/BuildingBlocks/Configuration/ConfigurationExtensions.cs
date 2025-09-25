using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Configuration;

/// <summary>
/// Extension methods for configuration services
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds configuration validation services
    /// </summary>
    public static IServiceCollection AddConfigurationValidation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register feature flag service
        services.AddSingleton<IFeatureFlagService, FeatureFlagService>();
        
        // Register configuration validation service
        services.AddSingleton<IConfigurationValidationService, ConfigurationValidationService>();
        
        return services;
    }

    /// <summary>
    /// Binds and validates a configuration section
    /// </summary>
    public static IServiceCollection ConfigureAndValidate<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName) where T : class, new()
    {
        var section = configuration.GetSection(sectionName);
        services.Configure<T>(section);
        
        // Validate configuration on startup
        services.AddSingleton<IValidateOptions<T>, ConfigurationValidator<T>>();
        
        return services;
    }

    /// <summary>
    /// Binds and validates a configuration section with custom validation
    /// </summary>
    public static IServiceCollection ConfigureAndValidate<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName,
        Func<T, bool> validationFunc,
        string errorMessage) where T : class, new()
    {
        var section = configuration.GetSection(sectionName);
        services.Configure<T>(section);
        
        // Add custom validation
        services.AddSingleton<IValidateOptions<T>>(new CustomConfigurationValidator<T>(validationFunc, errorMessage));
        
        return services;
    }
}

/// <summary>
/// Configuration validator for startup validation
/// </summary>
public class ConfigurationValidator<T> : IValidateOptions<T> where T : class
{
    public ValidateOptionsResult Validate(string? name, T options)
    {
        if (options is BaseConfiguration config)
        {
            if (!config.IsValid())
            {
                var errors = config.GetValidationErrors();
                return ValidateOptionsResult.Fail(errors);
            }
        }
        
        return ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Custom configuration validator
/// </summary>
public class CustomConfigurationValidator<T> : IValidateOptions<T> where T : class
{
    private readonly Func<T, bool> _validationFunc;
    private readonly string _errorMessage;

    public CustomConfigurationValidator(Func<T, bool> validationFunc, string errorMessage)
    {
        _validationFunc = validationFunc;
        _errorMessage = errorMessage;
    }

    public ValidateOptionsResult Validate(string? name, T options)
    {
        if (!_validationFunc(options))
        {
            return ValidateOptionsResult.Fail(_errorMessage);
        }
        
        return ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Configuration validation service implementation
/// </summary>
public class ConfigurationValidationService : IConfigurationValidationService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public ConfigurationValidationService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public async Task<ValidationResult> ValidateAllAsync()
    {
        var result = new ValidationResult { IsValid = true };
        
        // Validate all configuration sections
        var sections = _configuration.GetChildren();
        foreach (var section in sections)
        {
            var sectionResult = await ValidateSectionAsync(section.Key);
            if (!sectionResult.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(sectionResult.Errors);
            }
            result.Warnings.AddRange(sectionResult.Warnings);
        }
        
        return result;
    }

    public async Task<ValidationResult> ValidateSectionAsync(string sectionName)
    {
        var result = new ValidationResult { IsValid = true };
        
        try
        {
            var section = _configuration.GetSection(sectionName);
            if (!section.Exists())
            {
                result.AddWarning($"Configuration section '{sectionName}' does not exist");
                return result;
            }

            // Validate required values
            var requiredKeys = GetRequiredKeys(sectionName);
            foreach (var key in requiredKeys)
            {
                var value = section[key];
                if (string.IsNullOrEmpty(value))
                {
                    result.AddError($"Required configuration '{sectionName}:{key}' is missing or empty");
                }
            }
        }
        catch (Exception ex)
        {
            result.AddError($"Failed to validate configuration section '{sectionName}': {ex.Message}");
        }
        
        await Task.CompletedTask;
        return result;
    }

    private IEnumerable<string> GetRequiredKeys(string sectionName)
    {
        return sectionName.ToLowerInvariant() switch
        {
            "jwt" => new[] { "SecretKey", "Issuer", "Audience" },
            "database" => new[] { "ConnectionString" },
            "redis" => new[] { "ConnectionString" },
            "rabbitmq" => new[] { "Host", "UserName", "Password" },
            _ => Enumerable.Empty<string>()
        };
    }
}
