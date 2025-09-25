namespace BuildingBlocks.Auditing;

/// <summary>
/// Represents an audit entry for tracking system actions
/// </summary>
public class AuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The user who performed the action
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// The username/email of the user
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// The type of action performed
    /// </summary>
    public AuditType AuditType { get; set; }
    
    /// <summary>
    /// The entity type being audited
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// The ID of the entity being audited
    /// </summary>
    public string? EntityId { get; set; }
    
    /// <summary>
    /// The name of the action/operation
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional details about the action
    /// </summary>
    public string? Details { get; set; }
    
    /// <summary>
    /// The old values (for updates)
    /// </summary>
    public string? OldValues { get; set; }
    
    /// <summary>
    /// The new values (for updates)
    /// </summary>
    public string? NewValues { get; set; }
    
    /// <summary>
    /// The IP address of the user
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// The user agent string
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// When the action occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Whether the action was successful
    /// </summary>
    public bool IsSuccess { get; set; } = true;
    
    /// <summary>
    /// Error message if the action failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// The correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Types of audit actions
/// </summary>
public enum AuditType
{
    Create,
    Read,
    Update,
    Delete,
    Login,
    Logout,
    Authorization,
    Configuration,
    System
}
