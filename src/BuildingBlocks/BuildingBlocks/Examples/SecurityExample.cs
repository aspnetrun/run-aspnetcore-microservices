using BuildingBlocks.Security;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Examples;

/// <summary>
/// Example controller showing security building blocks usage
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SecurityExampleController : ControllerBase
{
    private readonly IJwtService _jwtService;

    public SecurityExampleController(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }

    /// <summary>
    /// Login endpoint - generates JWT token
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // In a real app, validate credentials here
        if (request.Email == "admin@example.com" && request.Password == "admin123")
        {
            var token = _jwtService.GenerateToken(
                userId: "admin-123",
                email: request.Email,
                roles: new[] { "Admin", "User" },
                additionalClaims: new Dictionary<string, string>
                {
                    { "Permission", "admin:access" },
                    { "Permission", "products:write" },
                    { "Permission", "orders:read" }
                });

            return Ok(new { token, message = "Login successful" });
        }

        return Unauthorized(new { message = "Invalid credentials" });
    }

    /// <summary>
    /// Protected endpoint requiring Admin role
    /// </summary>
    [HttpGet("admin-only")]
    [RequireAdmin]
    public IActionResult AdminOnly()
    {
        return Ok(new { message = "Admin access granted" });
    }

    /// <summary>
    /// Protected endpoint requiring specific permission
    /// </summary>
    [HttpGet("products")]
    [RequirePermission(Permissions.ProductsRead)]
    public IActionResult GetProducts()
    {
        return Ok(new { message = "Products access granted" });
    }

    /// <summary>
    /// Protected endpoint requiring multiple permissions
    /// </summary>
    [HttpPost("products")]
    [RequirePermission(Permissions.ProductsWrite, Permissions.ProductsRead)]
    public IActionResult CreateProduct([FromBody] CreateProductRequest request)
    {
        return Ok(new { message = "Product creation access granted", product = request });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
