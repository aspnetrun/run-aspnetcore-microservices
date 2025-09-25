using System.Security.Claims;

namespace BuildingBlocks.Security;

public interface IJwtService
{
    /// <summary>
    /// Generates a JWT token for the specified user
    /// </summary>
    string GenerateToken(string userId, string email, IEnumerable<string> roles, IDictionary<string, string>? additionalClaims = null);
    
    /// <summary>
    /// Validates a JWT token and returns the principal if valid
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);
    
    /// <summary>
    /// Extracts user ID from token claims
    /// </summary>
    string? GetUserIdFromToken(string token);
    
    /// <summary>
    /// Extracts roles from token claims
    /// </summary>
    IEnumerable<string> GetRolesFromToken(string token);
    
    /// <summary>
    /// Extracts email from token claims
    /// </summary>
    string? GetEmailFromToken(string token);
    
    /// <summary>
    /// Checks if a token is expired
    /// </summary>
    bool IsTokenExpired(string token);
    
    /// <summary>
    /// Refreshes a token if it's close to expiration
    /// </summary>
    string? RefreshToken(string token);
}
