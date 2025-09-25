using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BuildingBlocks.Auditing;

/// <summary>
/// Default implementation of the audit service
/// </summary>
public class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ILogger<AuditService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(AuditEntry entry)
    {
        try
        {
            // Enrich the entry with HTTP context information
            EnrichWithHttpContext(entry);
            
            // Log to structured logging
            _logger.LogInformation(
                "Audit: {Action} on {EntityType} {EntityId} by {User} at {Timestamp} - Success: {IsSuccess}",
                entry.Action,
                entry.EntityType,
                entry.EntityId ?? "N/A",
                entry.UserName ?? "Anonymous",
                entry.Timestamp,
                entry.IsSuccess);

            // TODO: In a real implementation, you would also:
            // 1. Save to database
            // 2. Send to external audit system
            // 3. Archive to long-term storage
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit entry");
        }
    }

    public async Task LogSuccessAsync(string action, string entityType, string? entityId = null, string? details = null)
    {
        var entry = new AuditEntry
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            AuditType = GetAuditTypeFromAction(action),
            IsSuccess = true
        };

        await LogAsync(entry);
    }

    public async Task LogFailureAsync(string action, string entityType, string? entityId = null, string? errorMessage = null, string? details = null)
    {
        var entry = new AuditEntry
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            ErrorMessage = errorMessage,
            AuditType = GetAuditTypeFromAction(action),
            IsSuccess = false
        };

        await LogAsync(entry);
    }

    public async Task LogCreateAsync(string entityType, string entityId, string? details = null)
    {
        await LogSuccessAsync("Create", entityType, entityId, details);
    }

    public async Task LogUpdateAsync(string entityType, string entityId, string? oldValues = null, string? newValues = null, string? details = null)
    {
        var entry = new AuditEntry
        {
            Action = "Update",
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            OldValues = oldValues,
            NewValues = newValues,
            AuditType = AuditType.Update,
            IsSuccess = true
        };

        await LogAsync(entry);
    }

    public async Task LogDeleteAsync(string entityType, string entityId, string? details = null)
    {
        await LogSuccessAsync("Delete", entityType, entityId, details);
    }

    public async Task LogReadAsync(string entityType, string? entityId = null, string? details = null)
    {
        await LogSuccessAsync("Read", entityType, entityId, details);
    }

    public async Task LogAuthorizationAsync(string action, string resource, bool isAuthorized, string? details = null)
    {
        var entry = new AuditEntry
        {
            Action = action,
            EntityType = "Authorization",
            EntityId = resource,
            Details = details,
            AuditType = AuditType.Authorization,
            IsSuccess = isAuthorized
        };

        await LogAsync(entry);
    }

    public async Task LogLoginAsync(string userId, string userName, bool isSuccess, string? details = null)
    {
        var entry = new AuditEntry
        {
            UserId = userId,
            UserName = userName,
            Action = "Login",
            EntityType = "Authentication",
            Details = details,
            AuditType = AuditType.Login,
            IsSuccess = isSuccess
        };

        await LogAsync(entry);
    }

    public async Task LogLogoutAsync(string userId, string userName, string? details = null)
    {
        var entry = new AuditEntry
        {
            UserId = userId,
            UserName = userName,
            Action = "Logout",
            EntityType = "Authentication",
            Details = details,
            AuditType = AuditType.Logout,
            IsSuccess = true
        };

        await LogAsync(entry);
    }

    private void EnrichWithHttpContext(AuditEntry entry)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        // Get user information from claims
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            entry.UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            entry.UserName = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst(ClaimTypes.Name)?.Value;
        }

        // Get IP address
        entry.IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? 
                         context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                         context.Request.Headers["X-Real-IP"].FirstOrDefault();

        // Get user agent
        entry.UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault();

        // Get correlation ID
        entry.CorrelationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ??
                             context.TraceIdentifier;
    }

    private AuditType GetAuditTypeFromAction(string action)
    {
        return action.ToLowerInvariant() switch
        {
            "create" => AuditType.Create,
            "read" => AuditType.Read,
            "update" => AuditType.Update,
            "delete" => AuditType.Delete,
            "login" => AuditType.Login,
            "logout" => AuditType.Logout,
            _ => AuditType.System
        };
    }
}
