using API.Abstractions.Data.Repositories;
using API.Data;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

/// <summary>
/// [ADDED|1.0.0] Product repository implementation
/// Extends generic Repository with product-specific filtering and querying
/// All complex queries are implemented here, keeping domain logic separated from generic CRUD
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    /// <summary>
    /// Initializes product repository with context
    /// </summary>
    public ProductRepository(StoreContext context) : base(context)
    {
    }

    /// <summary>
    /// [ADDED|1.0.0] Retrieves products with advanced filtering, sorting, and pagination
    /// All filtering happens at database level using IQueryable
    /// </summary>
    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredAsync(
        ProductFilterParams filter,
        CancellationToken cancellationToken = default)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        // Validate filter parameters
        if (filter.Page < 1)
            throw new ArgumentException("Page must be greater than 0", nameof(filter));

        if (filter.PageSize < 1 || filter.PageSize > 100)
            throw new ArgumentException("PageSize must be between 1 and 100", nameof(filter));

        // Start with base query (no tracking for read-only access)
        var query = GetAll();

        // Apply text search filter on name and description
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchLower = filter.SearchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchLower) ||
                                     p.Description.ToLower().Contains(searchLower));
        }

        // Apply brand filter
        if (!string.IsNullOrWhiteSpace(filter.Brand))
        {
            query = query.Where(p => p.Brand == filter.Brand);
        }

        // Apply type/category filter
        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            query = query.Where(p => p.Type == filter.Type);
        }

        // Apply price range filters
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        // Get total count before pagination (executes the where clauses)
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting based on sort field
        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        // Apply pagination (skip and take happen at database level)
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// [ADDED|1.0.0] Searches products by name or description
    /// Case-insensitive substring match
    /// </summary>
    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));

        var searchLower = searchTerm.ToLower();
        return await _dbSet.AsNoTracking()
            .Where(p => p.Name.ToLower().Contains(searchLower) ||
                       p.Description.ToLower().Contains(searchLower))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// [ADDED|1.0.0] Retrieves all products from a specific brand
    /// </summary>
    public async Task<IEnumerable<Product>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Brand cannot be empty", nameof(brand));

        return await _dbSet.AsNoTracking()
            .Where(p => p.Brand == brand)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// [ADDED|1.0.0] Retrieves all products of a specific type/category
    /// </summary>
    public async Task<IEnumerable<Product>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type cannot be empty", nameof(type));

        return await _dbSet.AsNoTracking()
            .Where(p => p.Type == type)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// [ADDED|1.0.0] Retrieves products within a specified price range
    /// Prices are in cents, filtering happens at database level
    /// </summary>
    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(long minPrice, long maxPrice, CancellationToken cancellationToken = default)
    {
        if (minPrice < 0 || maxPrice < 0)
            throw new ArgumentException("Prices cannot be negative");

        if (minPrice > maxPrice)
            throw new ArgumentException("Minimum price cannot be greater than maximum price");

        return await _dbSet.AsNoTracking()
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .OrderBy(p => p.Price)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// [ADDED|1.0.0] Retrieves products currently in stock (quantity > 0)
    /// </summary>
    public async Task<IEnumerable<Product>> GetInStockAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(p => p.QuantityInStock > 0)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// [ADDED|1.0.0] Retrieves products with low stock levels
    /// Useful for inventory alerts
    /// </summary>
    public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold, CancellationToken cancellationToken = default)
    {
        if (threshold < 0)
            throw new ArgumentException("Threshold cannot be negative", nameof(threshold));

        return await _dbSet.AsNoTracking()
            .Where(p => p.QuantityInStock <= threshold && p.QuantityInStock > 0)
            .OrderBy(p => p.QuantityInStock)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// [ADDED|1.0.0] Applies sorting to the query based on sort field and direction
    /// All sorting logic centralized here for maintainability
    /// </summary>
    /// <param name="query">The IQueryable to sort</param>
    /// <param name="sortBy">Field to sort by: name, price, stock, id</param>
    /// <param name="sortDirection">Direction: asc or desc</param>
    /// <returns>Sorted IQueryable</returns>
    private IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sortBy, string? sortDirection)
    {
        var isDescending = sortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false;

        return (sortBy?.ToLower()) switch
        {
            "price" => isDescending
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),

            "stock" => isDescending
                ? query.OrderByDescending(p => p.QuantityInStock)
                : query.OrderBy(p => p.QuantityInStock),

            "id" => isDescending
                ? query.OrderByDescending(p => p.Id)
                : query.OrderBy(p => p.Id),

            // Default to name
            _ => isDescending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name)
        };
    }
}
