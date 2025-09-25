using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BuildingBlocks.Security;

public static class SecurityExtensions
{
    /// <summary>
    /// Adds JWT authentication services to the service collection
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // Register JWT service
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }

    /// <summary>
    /// Adds authorization services with custom policies
    /// </summary>
    public static IServiceCollection AddCustomAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Add custom policies for permissions
            options.AddPolicy("Permission_products_read", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" && c.Value == "products:read")));

            options.AddPolicy("Permission_products_write", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" && c.Value == "products:write")));

            options.AddPolicy("Permission_orders_read", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" && c.Value == "orders:read")));

            options.AddPolicy("Permission_orders_write", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" && c.Value == "orders:write")));

            options.AddPolicy("Permission_basket_read", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" && c.Value == "basket:read")));

            options.AddPolicy("Permission_basket_write", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" && c.Value == "basket:write")));

            options.AddPolicy("Permission_admin_access", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" && c.Value == "admin:access")));
        });

        return services;
    }

    /// <summary>
    /// Adds security services (JWT + Authorization)
    /// </summary>
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddJwtAuthentication(configuration);
        services.AddCustomAuthorization();
        
        return services;
    }
}
