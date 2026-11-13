# Repository Pattern - Usage Examples

## Overview
This document demonstrates real-world usage of the repository pattern implementation.

## Example 1: Basic CRUD Operations

### Create a Product
```csharp
[HttpPost]
public async Task<ActionResult<ProductDto>> CreateProduct(
    [FromBody] CreateProductDto dto,
    CancellationToken cancellationToken)
{
    var product = await _productService.CreateAsync(dto, cancellationToken);
    return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
}
```

**Flow:**
1. Controller receives CreateProductDto
2. Validates request via dependency injection
3. Calls `IProductService.CreateAsync()`
4. Service validates business rules
5. Service calls `IProductRepository.AddAsync()`
6. Repository adds entity to DbSet
7. Repository calls `SaveChangesAsync()` to persist
8. Service maps Product entity to ProductDto
9. Controller returns 201 Created with location header

### Retrieve a Product
```csharp
public async Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken)
{
    var product = await _repository.GetByIdAsync(id, cancellationToken);
    if (product == null)
        throw new InvalidOperationException($"Product {id} not found");
    
    return MapToDto(product);
}
```

**Database Behavior:**
```sql
SELECT * FROM Products WHERE Id = @id LIMIT 1;
```

### Update a Product
```csharp
public async Task<ProductDto> UpdateAsync(
    int id, 
    UpdateProductDto dto, 
    CancellationToken cancellationToken)
{
    var product = await _repository.GetByIdAsync(id, cancellationToken);
    
    // Partial update - only update provided fields
    if (!string.IsNullOrWhiteSpace(dto.Name))
        product.Name = dto.Name;
    
    if (dto.Price.HasValue)
        product.Price = dto.Price.Value;
    
    await _repository.UpdateAsync(product, cancellationToken);
    await _repository.SaveChangesAsync(cancellationToken);
    
    return MapToDto(product);
}
```

### Delete a Product
```csharp
public async Task DeleteAsync(int id, CancellationToken cancellationToken)
{
    var product = await _repository.GetByIdAsync(id, cancellationToken);
    if (product == null)
        throw new InvalidOperationException($"Product {id} not found");
    
    await _repository.DeleteAsync(product, cancellationToken);
    await _repository.SaveChangesAsync(cancellationToken);
}
```

## Example 2: Filtering with Pagination

### HTTP Request
```
GET /api/products?brand=Nike&minPrice=5000&maxPrice=15000&page=1&pageSize=20&sortBy=price&sortDirection=asc
```

### Controller Action
```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
    [FromQuery] ProductFilterDto filter,
    CancellationToken cancellationToken)
{
    var result = await _productService.GetAllAsync(filter, cancellationToken);
    return Ok(result);
}
```

### Service Implementation
```csharp
public async Task<PagedResult<ProductDto>> GetAllAsync(
    ProductFilterDto filter, 
    CancellationToken cancellationToken = default)
{
    var filterParams = new ProductFilterParams
    {
        Page = filter.Page,                    // 1
        PageSize = filter.PageSize,            // 20
        Brand = filter.Brand,                  // Nike
        MinPrice = filter.MinPrice,            // 5000
        MaxPrice = filter.MaxPrice,            // 15000
        SortBy = filter.SortBy,                // price
        SortDirection = filter.SortDirection   // asc
    };

    var (items, totalCount) = await _repository.GetFilteredAsync(
        filterParams, 
        cancellationToken);

    var dtos = items.Select(MapToDto).ToList();
    return new PagedResult<ProductDto>(dtos, totalCount, filter.Page, filter.PageSize);
}
```

### Repository Implementation
```csharp
public async Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredAsync(
    ProductFilterParams filter,
    CancellationToken cancellationToken = default)
{
    // Start with all products (no tracking for read-only)
    var query = GetAll();  // AsNoTracking() included

    // Apply brand filter
    if (!string.IsNullOrWhiteSpace(filter.Brand))
    {
        query = query.Where(p => p.Brand == filter.Brand);
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

    // Get total count BEFORE pagination
    var totalCount = await query.CountAsync(cancellationToken);

    // Apply sorting
    query = ApplySorting(query, filter.SortBy, filter.SortDirection);

    // Apply pagination at database level
    var items = await query
        .Skip((filter.Page - 1) * filter.PageSize)
        .Take(filter.PageSize)
        .ToListAsync(cancellationToken);

    return (items, totalCount);
}
```

### Generated SQL Query
```sql
SELECT COUNT(*) 
FROM Products 
WHERE Brand = 'Nike' 
  AND Price >= 5000 
  AND Price <= 15000;

SELECT * 
FROM Products 
WHERE Brand = 'Nike' 
  AND Price >= 5000 
  AND Price <= 15000
ORDER BY Price ASC
LIMIT 20 OFFSET 0;
```

### Response
```json
{
  "items": [
    {
      "id": 1,
      "name": "Nike Air Max",
      "price": 8500,
      "brand": "Nike",
      ...
    },
    {
      "id": 2,
      "name": "Nike Cortez",
      "price": 9200,
      "brand": "Nike",
      ...
    }
  ],
  "totalCount": 45,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

## Example 3: Search Functionality

### HTTP Request
```
GET /api/products/search?q=running+shoes
```

### Repository Query
```csharp
public async Task<IEnumerable<Product>> SearchAsync(
    string searchTerm, 
    CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(searchTerm))
        throw new ArgumentException("Search term cannot be empty");

    var searchLower = searchTerm.ToLower();
    return await _dbSet.AsNoTracking()
        .Where(p => p.Name.ToLower().Contains(searchLower) ||
                   p.Description.ToLower().Contains(searchLower))
        .ToListAsync(cancellationToken);
}
```

### Generated SQL
```sql
SELECT * 
FROM Products 
WHERE LOWER(Name) LIKE '%running shoes%' 
   OR LOWER(Description) LIKE '%running shoes%';
```

## Example 4: Testing with Mocked Repository

### Unit Test
```csharp
[TestFixture]
public class ProductServiceTests
{
    private Mock<IProductRepository> _repositoryMock;
    private Mock<ILogger<ProductService>> _loggerMock;
    private ProductService _service;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<ProductService>>();
        _service = new ProductService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task GetByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product 
        { 
            Id = productId, 
            Name = "Test Product",
            Brand = "Test Brand",
            Price = 5000,
            QuantityInStock = 10
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual("Test Product", result.Name);
        _repositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetByIdAsync(999));
    }

    [Test]
    public async Task GetAllAsync_WithFilters_CallsRepositoryWithCorrectParams()
    {
        // Arrange
        var filter = new ProductFilterDto
        {
            Brand = "Nike",
            MinPrice = 5000,
            MaxPrice = 10000,
            Page = 1,
            PageSize = 10
        };

        var products = new[] 
        { 
            new Product { Id = 1, Name = "Nike 1", Brand = "Nike", Price = 6000 },
            new Product { Id = 2, Name = "Nike 2", Brand = "Nike", Price = 7000 }
        };

        _repositoryMock
            .Setup(r => r.GetFilteredAsync(It.IsAny<ProductFilterParams>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 2));

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.AreEqual(2, result.Items.Count());
        Assert.AreEqual(2, result.TotalCount);
        _repositoryMock.Verify(r => r.GetFilteredAsync(It.IsAny<ProductFilterParams>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

## Example 5: Swapping Implementations

### Original Configuration (SQLite)
```csharp
// Program.cs
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

### Switch to SQL Server (No other code changes!)
```csharp
// Program.cs - Just change one line
builder.Services.AddScoped<IProductRepository, SqlServerProductRepository>();

// New file: Data/Repositories/SqlServerProductRepository.cs
public class SqlServerProductRepository : Repository<Product>, IProductRepository
{
    public SqlServerProductRepository(StoreContext context) : base(context) { }
    
    // Same implementation as ProductRepository
    // Or add SQL Server-specific optimizations
}
```

**Result:** All services, controllers, and tests work unchanged! ✅

## Example 6: Complex Filtering

### User Wants: "Find all Nike shoes under $100, in stock, sorted by price"
```
GET /api/products?brand=Nike&type=shoes&maxPrice=10000&page=1&pageSize=50&sortBy=price&sortDirection=asc
```

### Behind the Scenes
```csharp
// Repository executes this database query:
var query = context.Products
    .AsNoTracking()
    .Where(p => p.Brand == "Nike")
    .Where(p => p.Type == "shoes")
    .Where(p => p.Price <= 10000)
    .Where(p => p.QuantityInStock > 0)
    .OrderBy(p => p.Price)
    .Skip(0)
    .Take(50);

var items = await query.ToListAsync();
```

**Entire filtering, sorting, and pagination happens at database level!**
Only 50 items transferred over network, not thousands.

## Key Patterns Used

1. **Repository Pattern** - Abstract data access behind interfaces
2. **Dependency Injection** - Inject abstractions, not implementations
3. **LINQ to Entities** - IQueryable for lazy evaluation
4. **Async/Await** - Non-blocking I/O throughout
5. **DTO Pattern** - Separate API models from domain models
6. **Service Layer** - Business logic separated from HTTP
7. **Unit of Work** - (Ready to implement) Coordinate multiple repositories
8. **Pagination** - Handle large datasets efficiently

## Performance Benefits

- ✅ Database-level filtering (no unnecessary data transfer)
- ✅ Pagination prevents loading entire tables
- ✅ Sorting at database (fast and efficient)
- ✅ Async/await prevents thread starvation
- ✅ AsNoTracking() for read-only queries (memory efficient)
- ✅ IQueryable composition (single query, no N+1 problems)

## Maintenance Benefits

- ✅ Easy to add new filtering criteria
- ✅ Easy to test with mocks
- ✅ Easy to switch database implementations
- ✅ Clear separation of concerns
- ✅ No circular dependencies
- ✅ Type-safe at compile time
