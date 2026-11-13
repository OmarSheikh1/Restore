using API.Entities;

namespace API.Abstractions.Data.Repositories;

/// <summary>
/// [ADDED|1.0.0] Product-specific repository interface
/// Extends generic IRepository with product-specific queries and filtering methods
/// Enables domain-specific operations while maintaining abstraction
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Retrieves products matching the specified filter criteria with pagination
    /// </summary>
    /// <param name="filter">Filter parameters including pagination, search, and sorting</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Paginated result set with metadata</returns>
    Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredAsync(
        ProductFilterParams filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for products by name or description
    /// </summary>
    /// <param name="searchTerm">Term to search for in product name and description</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Matching products</returns>
    /// <exception cref="ArgumentException">Thrown when search term is null or empty</exception>
    Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves products filtered by brand
    /// </summary>
    /// <param name="brand">Brand name to filter by</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Products matching the brand</returns>
    Task<IEnumerable<Product>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves products filtered by type/category
    /// </summary>
    /// <param name="type">Product type to filter by</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Products matching the type</returns>
    Task<IEnumerable<Product>> GetByTypeAsync(string type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves products within a price range (in cents)
    /// </summary>
    /// <param name="minPrice">Minimum price in cents</param>
    /// <param name="maxPrice">Maximum price in cents</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Products within the price range</returns>
    Task<IEnumerable<Product>> GetByPriceRangeAsync(long minPrice, long maxPrice, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves products with quantity in stock greater than zero
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>In-stock products</returns>
    Task<IEnumerable<Product>> GetInStockAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves products with low stock levels (quantity below threshold)
    /// </summary>
    /// <param name="threshold">Stock quantity threshold</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Low stock products</returns>
    Task<IEnumerable<Product>> GetLowStockAsync(int threshold, CancellationToken cancellationToken = default);
}

/// <summary>
/// [ADDED|1.0.0] Filter parameters for product queries
/// Contains all filtering, pagination, and sorting options
/// </summary>
public class ProductFilterParams
{
    /// <summary>Page number (1-based)</summary>
    public int Page { get; set; } = 1;

    /// <summary>Items per page</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Search term for name/description</summary>
    public string? SearchTerm { get; set; }

    /// <summary>Filter by brand</summary>
    public string? Brand { get; set; }

    /// <summary>Filter by type</summary>
    public string? Type { get; set; }

    /// <summary>Minimum price in cents</summary>
    public long? MinPrice { get; set; }

    /// <summary>Maximum price in cents</summary>
    public long? MaxPrice { get; set; }

    /// <summary>Field to sort by: name, price, stock, id</summary>
    public string? SortBy { get; set; } = "name";

    /// <summary>Sort direction: asc or desc</summary>
    public string? SortDirection { get; set; } = "asc";
}
