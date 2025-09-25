namespace BuildingBlocks.Auditing;

/// <summary>
/// Service for logging audit entries
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit entry
    /// </summary>
    Task LogAsync(AuditEntry entry);
    
    /// <summary>
    /// Logs a successful action
    /// </summary>
    Task LogSuccessAsync(string action, string entityType, string? entityId = null, string? details = null);
    
    /// <summary>
    /// Logs a failed action
    /// </summary>
    Task LogFailureAsync(string action, string entityType, string? entityId = null, string? errorMessage = null, string? details = null);
    
    /// <summary>
    /// Logs a create operation
    /// </summary>
    Task LogCreateAsync(string entityType, string entityId, string? details = null);
    
    /// <summary>
    /// Logs an update operation
    /// </summary>
    Task LogUpdateAsync(string entityType, string entityId, string? oldValues = null, string? newValues = null, string? details = null);
    
    /// <summary>
    /// Logs a delete operation
    /// </summary>
    Task LogDeleteAsync(string entityType, string entityId, string? details = null);
    
    /// <summary>
    /// Logs a read operation
    /// </summary>
    Task LogReadAsync(string entityType, string? entityId = null, string? details = null);
    
    /// <summary>
    /// Logs an authorization attempt
    /// </summary>
    Task LogAuthorizationAsync(string action, string resource, bool isAuthorized, string? details = null);
    
    /// <summary>
    /// Logs a login attempt
    /// </summary>
    Task LogLoginAsync(string userId, string userName, bool isSuccess, string? details = null);
    
    /// <summary>
    /// Logs a logout
    /// </summary>
    Task LogLogoutAsync(string userId, string userName, string? details = null);
}
