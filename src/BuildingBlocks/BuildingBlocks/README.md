# BuildingBlocks Library

A comprehensive collection of cross-cutting concerns and building blocks for .NET microservices architecture.

## 🏗️ **Overview**

BuildingBlocks provides reusable components, patterns, and abstractions that ensure consistency across your microservices. It follows the DRY principle and implements industry best practices for enterprise applications.

## 📦 **Available Building Blocks**

### **1. Security** 🔐
- **JWT Authentication & Authorization**
- **Role-based Access Control (RBAC)**
- **Permission-based Authorization**
- **Custom Authorization Attributes**

### **2. Auditing** 📝
- **Comprehensive Audit Logging**
- **Automatic Request/Response Auditing**
- **Audit Attributes for Controllers**
- **Structured Audit Entries**

### **3. Configuration** ⚙️
- **Configuration Validation**
- **Feature Flags**
- **Startup Validation**
- **Custom Validation Rules**

### **4. CQRS** 📚
- **Command & Query Interfaces**
- **MediatR Integration**
- **Handler Pattern Implementation**

### **5. Validation** ✅
- **FluentValidation Integration**
- **MediatR Pipeline Behaviors**
- **Automatic Validation**

### **6. Exception Handling** 🚨
- **Custom Exception Types**
- **Global Exception Handler**
- **Structured Error Responses**

### **7. Pagination** 📄
- **Generic Pagination Models**
- **Pagination Request/Response**

### **8. OpenTelemetry** 📊
- **Distributed Tracing**
- **Metrics Collection**
- **Logging Integration**

## 🚀 **Quick Start**

### **1. Install NuGet Packages**

```bash
dotnet add package YourCompany.BuildingBlocks
```

### **2. Configure Services**

```csharp
// Program.cs
using BuildingBlocks.Security;
using BuildingBlocks.Auditing;
using BuildingBlocks.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add Security Services
builder.Services.AddSecurityServices(builder.Configuration);

// Add Auditing Services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddHttpContextAccessor();

// Add Configuration Validation
builder.Services.AddConfigurationValidation(builder.Configuration);

// Configure and validate specific sections
builder.Services.ConfigureAndValidate<JwtConfiguration>(builder.Configuration, "Jwt");
builder.Services.ConfigureAndValidate<DatabaseConfiguration>(builder.Configuration, "Database");

var app = builder.Build();

// Configure middleware
app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

### **3. Configuration**

```json
// appsettings.json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-here",
    "Issuer": "your-app",
    "Audience": "your-app-users",
    "ExpirationHours": 24
  },
  "FeatureFlags": {
    "JwtAuthentication": true,
    "AuditLogging": true,
    "OpenTelemetry": true
  }
}
```

## 🔐 **Security Usage**

### **JWT Authentication**

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IJwtService _jwtService;

    public ProductsController(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var token = _jwtService.GenerateToken(
            userId: "user-123",
            email: request.Email,
            roles: new[] { "User" }
        );

        return Ok(new { token });
    }
}
```

### **Role-based Authorization**

```csharp
[HttpGet]
[RequireAdmin]
public IActionResult GetAdminData()
{
    return Ok(new { message = "Admin access granted" });
}

[HttpPost]
[RequireRole("Admin", "Manager")]
public IActionResult CreateResource()
{
    return Ok(new { message = "Resource created" });
}
```

### **Permission-based Authorization**

```csharp
[HttpGet]
[RequirePermission(Permissions.ProductsRead)]
public IActionResult GetProducts()
{
    return Ok(new { message = "Products access granted" });
}

[HttpPost]
[RequirePermission(Permissions.ProductsWrite, Permissions.ProductsRead)]
public IActionResult CreateProduct()
{
    return Ok(new { message = "Product creation access granted" });
}
```

## 📝 **Auditing Usage**

### **Automatic Auditing with Attributes**

```csharp
[HttpGet]
[AuditRead("Product", "Retrieve product list")]
public IActionResult GetProducts()
{
    // Automatically audited
    return Ok(products);
}

[HttpPost]
[AuditCreate("Product", "Create new product")]
public IActionResult CreateProduct([FromBody] CreateProductRequest request)
{
    // Automatically audited
    return Ok(createdProduct);
}
```

### **Manual Auditing**

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductRequest request)
{
    try
    {
        // Update logic here
        
        await _auditService.LogUpdateAsync("Product", id, oldValues, newValues, "Product updated");
        return Ok(updatedProduct);
    }
    catch (Exception ex)
    {
        await _auditService.LogFailureAsync("Update", "Product", id, ex.Message, "Update failed");
        return BadRequest();
    }
}
```

## ⚙️ **Configuration Usage**

### **Configuration Classes with Validation**

```csharp
[ConfigurationValidation(SectionName = "Database")]
public class DatabaseConfiguration : BaseConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetryCount { get; set; } = 3;

    public override bool IsValid()
    {
        return !string.IsNullOrEmpty(ConnectionString) && MaxRetryCount > 0;
    }

    public override IEnumerable<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrEmpty(ConnectionString))
            errors.Add("ConnectionString is required");
        
        if (MaxRetryCount <= 0)
            errors.Add("MaxRetryCount must be greater than 0");
        
        return errors;
    }
}
```

### **Feature Flags**

```csharp
public class ProductsController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlags;

    public ProductsController(IFeatureFlagService featureFlags)
    {
        _featureFlags = featureFlags;
    }

    [HttpGet]
    public IActionResult GetProducts()
    {
        if (!_featureFlags.IsEnabled(FeatureFlags.ProductsRead))
        {
            return NotFound("Feature not available");
        }

        // Feature is enabled, proceed with logic
        return Ok(products);
    }
}
```

## 🔧 **Advanced Configuration**

### **Custom Validation**

```csharp
// Program.cs
builder.Services.ConfigureAndValidate<CustomConfig>(
    builder.Configuration, 
    "CustomSection",
    config => config.Value > 0,
    "Value must be greater than 0"
);
```

### **Configuration Validation Service**

```csharp
public class StartupService
{
    private readonly IConfigurationValidationService _validationService;

    public StartupService(IConfigurationValidationService validationService)
    {
        _validationService = validationService;
    }

    public async Task ValidateConfigurationAsync()
    {
        var result = await _validationService.ValidateAllAsync();
        
        if (!result.IsValid)
        {
            throw new InvalidOperationException(
                $"Configuration validation failed: {string.Join(", ", result.Errors)}");
        }
    }
}
```

## 📊 **Monitoring & Observability**

### **OpenTelemetry Integration**

```csharp
// Program.cs
builder.Services.AddOpenTelemetryServices(
    builder.Configuration,
    serviceName: "MyService",
    serviceVersion: "1.0.0"
);
```

### **Health Checks**

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("Database")
    .AddCheck<RedisHealthCheck>("Redis");
```

## 🧪 **Testing**

### **Unit Testing Security**

```csharp
[Test]
public void JwtService_GenerateToken_ShouldCreateValidToken()
{
    // Arrange
    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Jwt:SecretKey"] = "test-secret-key",
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience"
        })
        .Build();
    
    var jwtService = new JwtService(config);
    
    // Act
    var token = jwtService.GenerateToken("user-123", "test@example.com", new[] { "User" });
    
    // Assert
    Assert.IsNotNull(token);
    Assert.IsTrue(token.Length > 0);
}
```

## 📚 **Best Practices**

### **1. Security**
- Always use strong secret keys
- Implement proper role and permission hierarchies
- Validate JWT tokens on every request
- Use HTTPS in production

### **2. Auditing**
- Audit all sensitive operations
- Include relevant context in audit entries
- Implement audit log retention policies
- Consider GDPR compliance

### **3. Configuration**
- Validate configuration on startup
- Use feature flags for gradual rollouts
- Implement configuration change notifications
- Secure sensitive configuration values

### **4. Performance**
- Use async/await for I/O operations
- Implement caching where appropriate
- Monitor and optimize database queries
- Use connection pooling

## 🔍 **Troubleshooting**

### **Common Issues**

1. **JWT Token Validation Fails**
   - Check secret key configuration
   - Verify issuer and audience settings
   - Ensure clock synchronization

2. **Audit Logging Not Working**
   - Verify IHttpContextAccessor is registered
   - Check audit service registration
   - Review logging configuration

3. **Configuration Validation Errors**
   - Check required configuration values
   - Verify configuration section names
   - Review custom validation logic

## 📖 **API Reference**

For detailed API documentation, see the individual namespace documentation:

- [BuildingBlocks.Security](Security/README.md)
- [BuildingBlocks.Auditing](Auditing/README.md)
- [BuildingBlocks.Configuration](Configuration/README.md)
- [BuildingBlocks.CQRS](CQRS/README.md)
- [BuildingBlocks.Validation](Behaviors/README.md)
- [BuildingBlocks.Exceptions](Exceptions/README.md)

## 🤝 **Contributing**

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
