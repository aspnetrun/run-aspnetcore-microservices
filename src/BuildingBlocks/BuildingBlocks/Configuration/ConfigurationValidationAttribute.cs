using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Configuration;

/// <summary>
/// Attribute to mark configuration classes for automatic validation
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ConfigurationValidationAttribute : Attribute
{
    /// <summary>
    /// The configuration section name
    /// </summary>
    public string SectionName { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to validate on startup
    /// </summary>
    public bool ValidateOnStartup { get; set; } = true;
}

/// <summary>
/// Base configuration class with validation support
/// </summary>
public abstract class BaseConfiguration
{
    /// <summary>
    /// Validates the configuration
    /// </summary>
    public virtual bool IsValid()
    {
        return true;
    }
    
    /// <summary>
    /// Gets validation errors
    /// </summary>
    public virtual IEnumerable<string> GetValidationErrors()
    {
        return Enumerable.Empty<string>();
    }
}

/// <summary>
/// Configuration validation service
/// </summary>
public interface IConfigurationValidationService
{
    /// <summary>
    /// Validates all registered configurations
    /// </summary>
    Task<ValidationResult> ValidateAllAsync();
    
    /// <summary>
    /// Validates a specific configuration section
    /// </summary>
    Task<ValidationResult> ValidateSectionAsync(string sectionName);
}

/// <summary>
/// Configuration validation result
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }
    
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}
