using API.Abstractions.Data.Repositories;
using API.Abstractions.Services;
using API.Dtos;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

/// <summary>
/// [ADDED|1.0.0] Product service implementing business logic for product operations
/// Coordinates repository access, data transformation, and business rule validation
/// All CRUD operations go through this service layer, not directly to repository
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes product service with dependencies
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when dependencies are null</exception>
    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// [ADDED|1.0.0] Retrieves product by ID and maps to DTO
    /// </summary>
    public async Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Product ID must be greater than 0", nameof(id));

        var product = await _repository.GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            throw new InvalidOperationException($"Product with ID {id} not found");
        }

        _logger.LogInformation("Retrieved product: {ProductId}", id);
        return MapToDto(product);
    }

    /// <summary>
    /// [ADDED|1.0.0] Retrieves paginated product list with filtering and sorting
    /// Delegates filtering to repository for database-level optimization
    /// </summary>
    public async Task<PagedResult<ProductDto>> GetAllAsync(ProductFilterDto filter, CancellationToken cancellationToken = default)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        // Validate filter parameters
        if (!filter.IsValid(out var errorMessage))
            throw new ArgumentException(errorMessage, nameof(filter));

        try
        {
            // Convert DTO to repository filter parameters
            var filterParams = new ProductFilterParams
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                SearchTerm = filter.SearchTerm,
                Brand = filter.Brand,
                Type = filter.Type,
                MinPrice = filter.MinPrice,
                MaxPrice = filter.MaxPrice,
                SortBy = filter.SortBy,
                SortDirection = filter.SortDirection
            };

            // Execute filtered query at database level
            var (items, totalCount) = await _repository.GetFilteredAsync(filterParams, cancellationToken);

            // Map entities to DTOs
            var dtos = items.Select(MapToDto).ToList();

            _logger.LogInformation(
                "Retrieved {Count} products (page {Page} of {TotalPages})",
                items.Count(),
                filter.Page,
                (totalCount + filter.PageSize - 1) / filter.PageSize);

            return new PagedResult<ProductDto>(dtos, totalCount, filter.Page, filter.PageSize);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid filter parameters provided");
            throw;
        }
    }

    /// <summary>
    /// [ADDED|1.0.0] Searches products by name or description
    /// </summary>
    public async Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));

        var products = await _repository.SearchAsync(searchTerm, cancellationToken);

        _logger.LogInformation("Search performed: {SearchTerm}, found {Count} results", searchTerm, products.Count());

        return products.Select(MapToDto);
    }

    /// <summary>
    /// [ADDED|1.0.0] Retrieves products from specific brand
    /// </summary>
    public async Task<IEnumerable<ProductDto>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Brand cannot be empty", nameof(brand));

        var products = await _repository.GetByBrandAsync(brand, cancellationToken);

        _logger.LogInformation("Retrieved {Count} products from brand: {Brand}", products.Count(), brand);

        return products.Select(MapToDto);
    }

    /// <summary>
    /// [ADDED|1.0.0] Retrieves products of specific type
    /// </summary>
    public async Task<IEnumerable<ProductDto>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type cannot be empty", nameof(type));

        var products = await _repository.GetByTypeAsync(type, cancellationToken);

        _logger.LogInformation("Retrieved {Count} products of type: {Type}", products.Count(), type);

        return products.Select(MapToDto);
    }

    /// <summary>
    /// [ADDED|1.0.0] Retrieves products within price range
    /// </summary>
    public async Task<IEnumerable<ProductDto>> GetByPriceRangeAsync(long minPrice, long maxPrice, CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetByPriceRangeAsync(minPrice, maxPrice, cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} products in price range: ${MinPrice}-${MaxPrice}",
            products.Count(),
            minPrice / 100m,
            maxPrice / 100m);

        return products.Select(MapToDto);
    }

    /// <summary>
    /// [ADDED|1.0.0] Retrieves in-stock products
    /// </summary>
    public async Task<IEnumerable<ProductDto>> GetInStockAsync(CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetInStockAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} in-stock products", products.Count());

        return products.Select(MapToDto);
    }

    /// <summary>
    /// [ADDED|1.0.0] Creates new product with validation
    /// </summary>
    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // Validate required fields
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Product name is required", nameof(dto.Name));

        if (dto.Price < 0)
            throw new ArgumentException("Product price cannot be negative", nameof(dto.Price));

        if (dto.QuantityInStock < 0)
            throw new ArgumentException("Quantity in stock cannot be negative", nameof(dto.QuantityInStock));

        // Create entity from DTO
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            PictureUrl = dto.PictureUrl,
            Type = dto.Type,
            Brand = dto.Brand,
            QuantityInStock = dto.QuantityInStock
        };

        // Persist to database
        await _repository.AddAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created: {ProductId} - {ProductName}", product.Id, product.Name);

        return MapToDto(product);
    }

    /// <summary>
    /// [ADDED|1.0.0] Updates existing product
    /// Only updates fields that are explicitly provided (not null in DTO)
    /// </summary>
    public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Product ID must be greater than 0", nameof(id));

        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // Retrieve existing product
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Attempted to update non-existent product: {ProductId}", id);
            throw new InvalidOperationException($"Product with ID {id} not found");
        }

        // Update only provided fields (partial update)
        if (!string.IsNullOrWhiteSpace(dto.Name))
            product.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Description))
            product.Description = dto.Description;

        if (dto.Price.HasValue)
        {
            if (dto.Price < 0)
                throw new ArgumentException("Product price cannot be negative", nameof(dto.Price));
            product.Price = dto.Price.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.PictureUrl))
            product.PictureUrl = dto.PictureUrl;

        if (!string.IsNullOrWhiteSpace(dto.Type))
            product.Type = dto.Type;

        if (!string.IsNullOrWhiteSpace(dto.Brand))
            product.Brand = dto.Brand;

        if (dto.QuantityInStock.HasValue)
        {
            if (dto.QuantityInStock < 0)
                throw new ArgumentException("Quantity in stock cannot be negative", nameof(dto.QuantityInStock));
            product.QuantityInStock = dto.QuantityInStock.Value;
        }

        // Persist changes
        await _repository.UpdateAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product updated: {ProductId} - {ProductName}", id, product.Name);

        return MapToDto(product);
    }

    /// <summary>
    /// [ADDED|1.0.0] Deletes product by ID
    /// </summary>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Product ID must be greater than 0", nameof(id));

        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Attempted to delete non-existent product: {ProductId}", id);
            throw new InvalidOperationException($"Product with ID {id} not found");
        }

        await _repository.DeleteAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product deleted: {ProductId} - {ProductName}", id, product.Name);
    }

    /// <summary>
    /// [ADDED|1.0.0] Maps Product entity to ProductDto
    /// Centralized mapping logic for consistency
    /// </summary>
    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            PictureUrl = product.PictureUrl,
            Type = product.Type,
            Brand = product.Brand,
            QuantityInStock = product.QuantityInStock
        };
    }
}
