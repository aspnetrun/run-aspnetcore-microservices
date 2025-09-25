namespace BuildingBlocks.Auditing;

/// <summary>
/// Attribute to automatically audit an action
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AuditAttribute : Attribute
{
    /// <summary>
    /// The type of audit action
    /// </summary>
    public AuditType AuditType { get; set; }
    
    /// <summary>
    /// The entity type being audited
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to audit the request body
    /// </summary>
    public bool AuditRequestBody { get; set; } = false;
    
    /// <summary>
    /// Whether to audit the response body
    /// </summary>
    public bool AuditResponseBody { get; set; } = false;
    
    /// <summary>
    /// Custom action name for the audit
    /// </summary>
    public string? ActionName { get; set; }
    
    /// <summary>
    /// Additional details for the audit
    /// </summary>
    public string? Details { get; set; }
}

/// <summary>
/// Attribute to audit create operations
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuditCreateAttribute : AuditAttribute
{
    public AuditCreateAttribute(string entityType, string? details = null)
    {
        AuditType = AuditType.Create;
        EntityType = entityType;
        Details = details;
        AuditRequestBody = true;
    }
}

/// <summary>
/// Attribute to audit read operations
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuditReadAttribute : AuditAttribute
{
    public AuditReadAttribute(string entityType, string? details = null)
    {
        AuditType = AuditType.Read;
        EntityType = entityType;
        Details = details;
    }
}

/// <summary>
/// Attribute to audit update operations
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuditUpdateAttribute : AuditAttribute
{
    public AuditUpdateAttribute(string entityType, string? details = null)
    {
        AuditType = AuditType.Update;
        EntityType = entityType;
        Details = details;
        AuditRequestBody = true;
    }
}

/// <summary>
/// Attribute to audit delete operations
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuditDeleteAttribute : AuditAttribute
{
    public AuditDeleteAttribute(string entityType, string? details = null)
    {
        AuditType = AuditType.Delete;
        EntityType = entityType;
        Details = details;
    }
}
