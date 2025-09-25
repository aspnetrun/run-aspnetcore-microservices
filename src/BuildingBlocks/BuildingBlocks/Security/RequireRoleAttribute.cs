using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Security;

/// <summary>
/// Attribute to require specific roles for access
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}

/// <summary>
/// Attribute to require admin role for access
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAdminAttribute : RequireRoleAttribute
{
    public RequireAdminAttribute() : base("Admin") { }
}

/// <summary>
/// Attribute to require user role for access
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireUserAttribute : RequireRoleAttribute
{
    public RequireUserAttribute() : base("User") { }
}
