using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Security;

/// <summary>
/// Attribute to require specific permissions for access
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(params string[] permissions)
    {
        Policy = $"Permission_{string.Join("_", permissions)}";
    }
}

/// <summary>
/// Common permission constants
/// </summary>
public static class Permissions
{
    // Product permissions
    public const string ProductsRead = "products:read";
    public const string ProductsWrite = "products:write";
    public const string ProductsDelete = "products:delete";
    
    // Order permissions
    public const string OrdersRead = "orders:read";
    public const string OrdersWrite = "orders:write";
    public const string OrdersDelete = "orders:delete";
    
    // Basket permissions
    public const string BasketRead = "basket:read";
    public const string BasketWrite = "basket:write";
    public const string BasketDelete = "basket:delete";
    
    // User management permissions
    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";
    public const string UsersDelete = "users:delete";
    
    // Admin permissions
    public const string AdminAccess = "admin:access";
    public const string SystemConfig = "system:config";
}
