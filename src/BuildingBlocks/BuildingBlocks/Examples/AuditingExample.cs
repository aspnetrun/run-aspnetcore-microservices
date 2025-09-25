using BuildingBlocks.Auditing;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Examples;

/// <summary>
/// Example controller showing auditing building blocks usage
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuditingExampleController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditingExampleController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>
    /// Example with automatic auditing using attributes
    /// </summary>
    [HttpGet]
    [AuditRead("Product", "Retrieve product list")]
    public IActionResult GetProducts()
    {
        // The action will be automatically audited
        return Ok(new { message = "Products retrieved successfully" });
    }

    /// <summary>
    /// Example with manual auditing
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            // Simulate product creation
            var productId = Guid.NewGuid().ToString();
            
            // Log successful creation
            await _auditService.LogCreateAsync("Product", productId, $"Product '{request.Name}' created");
            
            return Ok(new { id = productId, message = "Product created successfully" });
        }
        catch (Exception ex)
        {
            // Log failed creation
            await _auditService.LogFailureAsync("Create", "Product", null, ex.Message, "Product creation failed");
            return BadRequest(new { message = "Failed to create product" });
        }
    }

    /// <summary>
    /// Example with update auditing
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            // Simulate product update
            var oldValues = "{ \"name\": \"Old Product\", \"price\": 10.99 }";
            var newValues = System.Text.Json.JsonSerializer.Serialize(request);
            
            // Log the update with old and new values
            await _auditService.LogUpdateAsync("Product", id, oldValues, newValues, "Product updated");
            
            return Ok(new { message = "Product updated successfully" });
        }
        catch (Exception ex)
        {
            await _auditService.LogFailureAsync("Update", "Product", id, ex.Message, "Product update failed");
            return BadRequest(new { message = "Failed to update product" });
        }
    }

    /// <summary>
    /// Example with authorization auditing
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        // Simulate authorization check
        var isAuthorized = User.IsInRole("Admin");
        
        // Log authorization attempt
        await _auditService.LogAuthorizationAsync("Delete", $"Product:{id}", isAuthorized, "Admin role required");
        
        if (!isAuthorized)
        {
            return Forbid();
        }

        // Log successful deletion
        await _auditService.LogDeleteAsync("Product", id, "Product deleted by admin");
        
        return Ok(new { message = "Product deleted successfully" });
    }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
