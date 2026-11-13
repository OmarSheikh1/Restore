using API.Dtos;

namespace API.Abstractions.Services;

/// <summary>
/// [ADDED|1.0.0] Product service interface defining high-level product operations
/// Services orchestrate repository operations and apply business logic
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves a product by ID with DTO mapping
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>Product DTO if found</returns>
    /// <exception cref="InvalidOperationException">Thrown when product not found</exception>
    Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all products with filtering, sorting, and pagination
    /// </summary>
    /// <param name="filter">Filter parameters for query customization</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>Paginated result with product DTOs</returns>
    Task<PagedResult<ProductDto>> GetAllAsync(ProductFilterDto filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches products by name or description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>Collection of matching product DTOs</returns>
    Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves products from a specific brand
    /// </summary>
    /// <param name="brand">Brand name</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>Collection of product DTOs</returns>
    Task<IEnumerable<ProductDto>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves products of a specific type/category
    /// </summary>
    /// <param name="type">Product type</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>Collection of product DTOs</returns>
    Task<IEnumerable<ProductDto>> GetByTypeAsync(string type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves products within a price range
    /// </summary>
    /// <param name="minPrice">Minimum price in cents</param>
    /// <param name="maxPrice">Maximum price in cents</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>Collection of product DTOs</returns>
    Task<IEnumerable<ProductDto>> GetByPriceRangeAsync(long minPrice, long maxPrice, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves products currently in stock
    /// </summary>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>Collection of in-stock product DTOs</returns>
    Task<IEnumerable<ProductDto>> GetInStockAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="dto">Product creation data</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>Created product DTO with ID</returns>
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">Product ID to update</param>
    /// <param name="dto">Updated product data</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>Updated product DTO</returns>
    /// <exception cref="InvalidOperationException">Thrown when product not found</exception>
    Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">Product ID to delete</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <exception cref="InvalidOperationException">Thrown when product not found</exception>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
