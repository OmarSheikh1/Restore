using API.Abstractions.Services;
using API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// [ADDED|1.0.0] Products API controller
/// Handles HTTP requests for product operations
/// All business logic delegated to IProductService
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    /// <summary>
    /// [ADDED|1.0.0] Initializes controller with injected service dependencies
    /// </summary>
    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// [ADDED|1.0.0] GET /api/products - Retrieves paginated product list with optional filtering
    /// Supports filtering by search term, brand, type, and price range
    /// Supports sorting and pagination
    /// </summary>
    /// <param name="filter">Query parameters for filtering, sorting, and pagination</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] ProductFilterDto filter,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _productService.GetAllAsync(filter, cancellationToken);
            _logger.LogInformation("Products retrieved: {Count} items", result.Items.Count());
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid filter parameters");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// [ADDED|1.0.0] GET /api/products/{id} - Retrieves a specific product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>Product details if found</returns>
    [HttpGet("{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id, cancellationToken);
            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Product not found: {ProductId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid product ID: {ProductId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// [ADDED|1.0.0] GET /api/products/search - Searches products by name or description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>List of matching products</returns>
    [HttpGet("search")]
    [Produces("application/json")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Search(
        [FromQuery(Name = "q")] string searchTerm,
        CancellationToken cancellationToken)
    {
        try
        {
            var results = await _productService.SearchAsync(searchTerm, cancellationToken);
            return Ok(results);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// [ADDED|1.0.0] GET /api/products/brand/{brand} - Retrieves products by brand
    /// </summary>
    [HttpGet("brand/{brand}")]
    [Produces("application/json")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByBrand(
        string brand,
        CancellationToken cancellationToken)
    {
        var products = await _productService.GetByBrandAsync(brand, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// [ADDED|1.0.0] GET /api/products/type/{type} - Retrieves products by type
    /// </summary>
    [HttpGet("type/{type}")]
    [Produces("application/json")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByType(
        string type,
        CancellationToken cancellationToken)
    {
        var products = await _productService.GetByTypeAsync(type, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// [ADDED|1.0.0] GET /api/products/instock - Retrieves all in-stock products
    /// </summary>
    [HttpGet("instock")]
    [Produces("application/json")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetInStock(CancellationToken cancellationToken)
    {
        var products = await _productService.GetInStockAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// [ADDED|1.0.0] POST /api/products - Creates a new product
    /// </summary>
    [HttpPost]
    [Produces("application/json")]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (ArgumentNullException)
        {
            return BadRequest(new { error = "Request body is required" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid product data provided");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// [ADDED|1.0.0] PUT /api/products/{id} - Updates an existing product
    /// </summary>
    [HttpPut("{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        int id,
        [FromBody] UpdateProductDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.UpdateAsync(id, dto, cancellationToken);
            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Product not found for update: {ProductId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid update data for product: {ProductId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// [ADDED|1.0.0] DELETE /api/products/{id} - Deletes a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _productService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Product not found for deletion: {ProductId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid product ID: {ProductId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }
}