# [ADDED|1.0.0] Repository Pattern Implementation with Abstraction Layers

## Overview

The repository pattern has been fully implemented across the application with strict adherence to:
- ✅ **Dependency Inversion Principle** - All dependencies injected through interfaces
- ✅ **Abstraction Layers** - Complete separation of abstractions and implementations
- ✅ **Folder Structure** - Organized by responsibility
- ✅ **IQueryable Filtering** - Database-level query evaluation
- ✅ **Async/Await** - Full async support with CancellationToken

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                        HTTP Requests                                 │
└───────────────────────────────┬─────────────────────────────────────┘
                                │
                                ▼
                    ┌──────────────────────┐
                    │  Controllers/        │
                    │  ProductsController  │
                    │  (HTTP Only)         │
                    └──────────┬───────────┘
                               │ (depends on)
                               ▼
                    ┌──────────────────────────┐
                    │  Abstractions/Services/  │
                    │  IProductService         │
                    │  (Interface)             │
                    └──────────┬───────────────┘
                               │ (depends on)
                               ▼
                    ┌──────────────────────────┐
                    │  Services/               │
                    │  ProductService          │
                    │  (Business Logic)        │
                    └──────────┬───────────────┘
                               │ (depends on)
                               ▼
                    ┌──────────────────────────┐
                    │  Abstractions/Data/      │
                    │  Repositories/           │
                    │  IProductRepository      │
                    │  (Interface)             │
                    └──────────┬───────────────┘
                               │ (depends on)
                               ▼
                    ┌──────────────────────────┐
                    │  Data/Repositories/      │
                    │  ProductRepository       │
                    │  (Data Access)           │
                    └──────────┬───────────────┘
                               │ (uses)
                               ▼
                    ┌──────────────────────────┐
                    │  StoreContext            │
                    │  (DbContext)             │
                    └──────────┬───────────────┘
                               │ (queries)
                               ▼
                    ┌──────────────────────────┐
                    │  SQLite Database         │
                    └──────────────────────────┘
```

## Project Structure

```
API/
├── Abstractions/                        ← Interfaces ONLY
│   ├── Data/
│   │   └── Repositories/
│   │       ├── IRepository.cs           ← Generic repository interface
│   │       └── IProductRepository.cs    ← Product-specific interface
│   └── Services/
│       └── IProductService.cs           ← Product service interface
│
├── Data/                                ← Implementation layer
│   ├── StoreContext.cs                  ← Entity Framework DbContext
│   ├── Repositories/
│   │   ├── Repository.cs                ← Generic base implementation
│   │   └── ProductRepository.cs         ← Product-specific implementation
│   └── Migrations/
│       ├── 20261113174630_InitialCreate.cs
│       ├── 20261113174630_InitialCreate.Designer.cs
│       └── StoreContextModelSnapshot.cs
│
├── Services/                            ← Business logic layer
│   └── ProductService.cs                ← Product service implementation
│
├── Dtos/                                ← Data transfer objects
│   ├── ProductDto.cs                    ← Product DTOs
│   ├── ProductFilterDto.cs              ← Filtering parameters
│   └── PagedResult.cs                   ← Pagination wrapper
│
├── Entities/                            ← Domain models
│   ├── Products.cs
│   └── User.cs
│
├── Controllers/                         ← HTTP endpoints
│   └── ProductsController.cs            ← Product API controller
│
├── Program.cs                           ← Dependency injection setup
└── API.csproj                           ← Project configuration
```

## Key Components

### 1. Generic Repository Interface - `IRepository<T>`

**Location:** `Abstractions/Data/Repositories/IRepository.cs`

Provides base CRUD operations for any entity:
- `GetByIdAsync(int id)` - Retrieve by primary key
- `GetAll()` - Get all entities as IQueryable
- `Find(Expression<Func<T, bool>> predicate)` - Filter with lazy evaluation
- `AddAsync(T entity)` - Add single entity
- `AddRangeAsync(IEnumerable<T> entities)` - Add batch
- `UpdateAsync(T entity)` - Update entity
- `DeleteAsync(T entity)` - Delete entity
- `ExistsAsync(Expression predicate)` - Check existence
- `CountAsync(predicate)` - Count matching entities
- `SaveChangesAsync()` - Persist changes

### 2. Product-Specific Repository - `IProductRepository`

**Location:** `Abstractions/Data/Repositories/IProductRepository.cs`

Extends `IRepository<Product>` with product-specific queries:
- `GetFilteredAsync(ProductFilterParams filter)` - Advanced filtering with pagination
- `SearchAsync(string searchTerm)` - Search by name/description
- `GetByBrandAsync(string brand)` - Filter by brand
- `GetByTypeAsync(string type)` - Filter by type/category
- `GetByPriceRangeAsync(long min, long max)` - Price range filtering
- `GetInStockAsync()` - Get available products
- `GetLowStockAsync(int threshold)` - Get low stock items

### 3. Generic Repository Implementation

**Location:** `Data/Repositories/Repository.cs`

Base implementation of IRepository<T>:
- All queries use `AsNoTracking()` for read-only access
- Filtering via IQueryable (lazy evaluation at DB level)
- Null checks and validation on all inputs
- Async/await throughout

### 4. Product Repository Implementation

**Location:** `Data/Repositories/ProductRepository.cs`

Implements product-specific repository interface:

```csharp
public class ProductRepository : Repository<Product>, IProductRepository
{
    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredAsync(
        ProductFilterParams filter,
        CancellationToken cancellationToken = default)
    {
        var query = GetAll();
        
        // Apply text search
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchLower = filter.SearchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchLower) ||
                                     p.Description.ToLower().Contains(searchLower));
        }
        
        // Apply filters (brand, type, price range)
        // ...
        
        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);
        
        // Apply sorting
        query = ApplySorting(query, filter.SortBy, filter.SortDirection);
        
        // Apply pagination
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);
        
        return (items, totalCount);
    }
}
```

### 5. Service Layer - `IProductService`

**Location:** `Abstractions/Services/IProductService.cs`

High-level business operations:
- `GetByIdAsync(int id)` - Get single product
- `GetAllAsync(ProductFilterDto filter)` - Get paginated filtered list
- `SearchAsync(string searchTerm)` - Search products
- `GetByBrandAsync(string brand)` - Get by brand
- `GetByTypeAsync(string type)` - Get by type
- `GetByPriceRangeAsync(long min, long max)` - Price range
- `GetInStockAsync()` - In-stock products
- `CreateAsync(CreateProductDto)` - Create new
- `UpdateAsync(int id, UpdateProductDto)` - Update existing
- `DeleteAsync(int id)` - Delete product

### 6. Service Implementation

**Location:** `Services/ProductService.cs`

Orchestrates repository and business logic:

```csharp
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<ProductDto>> GetAllAsync(
        ProductFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        // Validate filter
        // Convert DTO to repository parameters
        // Execute filtered query at database level
        // Map entities to DTOs
        // Return paginated results
    }
}
```

### 7. Controllers - HTTP Only

**Location:** `Controllers/ProductsController.cs`

Controllers only handle HTTP concerns:
- Receive requests from clients
- Validate input parameters
- Call service methods
- Map responses
- Handle exceptions
- Return HTTP status codes

```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
    [FromQuery] ProductFilterDto filter,
    CancellationToken cancellationToken)
{
    try
    {
        var result = await _productService.GetAllAsync(filter, cancellationToken);
        return Ok(result);
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

### 8. DTOs - Data Transfer Objects

**Location:** `Dtos/`

- `ProductDto` - API response model
- `CreateProductDto` - Create request model
- `UpdateProductDto` - Update request model
- `ProductFilterDto` - Filtering parameters with validation
- `PagedResult<T>` - Paginated response wrapper

## Dependency Injection Configuration

**Location:** `Program.cs`

```csharp
// Register DbContext
builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Register repositories (abstraction → implementation)
builder.Services.AddScoped<IRepository<Product>, Repository<Product>>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Register services (abstraction → implementation)
builder.Services.AddScoped<IProductService, ProductService>();

// Register logging and Swagger
builder.Services.AddLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

## API Endpoints

All endpoints use the service layer and repository pattern:

### GET /api/products
Retrieve paginated product list with filtering

**Query Parameters:**
- `page` (int, default: 1) - Page number
- `pageSize` (int, default: 10, max: 100) - Items per page
- `searchTerm` (string) - Search by name/description
- `brand` (string) - Filter by brand
- `type` (string) - Filter by type
- `minPrice` (long) - Minimum price in cents
- `maxPrice` (long) - Maximum price in cents
- `sortBy` (string) - Sort field: name, price, stock, id
- `sortDirection` (string) - asc or desc

**Response:**
```json
{
  "items": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 10,
  "totalPages": 15,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### GET /api/products/{id}
Retrieve single product

### GET /api/products/search?q=searchterm
Search products

### GET /api/products/brand/{brand}
Get products by brand

### GET /api/products/type/{type}
Get products by type

### GET /api/products/instock
Get in-stock products

### POST /api/products
Create new product

### PUT /api/products/{id}
Update existing product

### DELETE /api/products/{id}
Delete product

## Benefits of This Architecture

### ✅ Testability
Services can be tested with mocked repositories:
```csharp
var repositoryMock = new Mock<IProductRepository>();
var service = new ProductService(repositoryMock.Object, loggerMock.Object);
// Test service logic in isolation
```

### ✅ Maintainability
Clear separation of concerns:
- Controllers handle HTTP
- Services handle business logic
- Repositories handle data access
- Each layer has single responsibility

### ✅ Swappability
Switch implementations without changing client code:
```csharp
// Change from SQLite to SQL Server
builder.Services.AddScoped<IProductRepository, SqlServerProductRepository>();
// All services and controllers work unchanged
```

### ✅ Async Throughout
Full async/await support with CancellationToken:
- Database queries are truly asynchronous
- No blocking threads
- Graceful cancellation support

### ✅ Database-Level Filtering
IQueryable enables LINQ provider translation:
- Filters applied at database level
- No unnecessary data transfer
- Optimal performance
- Sorting, pagination at DB

### ✅ Validation & Error Handling
Input validation at service layer:
- Null checks on all dependencies
- Explicit exceptions on invalid input
- Logging of all operations
- Graceful error responses

## Filtering & Querying Examples

### Example 1: Filter by Brand and Price Range
```csharp
GET /api/products?brand=Nike&minPrice=5000&maxPrice=15000&sortBy=price&sortDirection=asc
```

### Example 2: Search with Pagination
```csharp
GET /api/products?searchTerm=running&page=2&pageSize=20
```

### Example 3: Complex Filter
```csharp
GET /api/products?type=shoes&brand=Adidas&minPrice=3000&maxPrice=10000&sortBy=price
```

All filters execute at the database level through IQueryable LINQ translation!

## Code Labeling

All changes are labeled with `[ADDED|1.0.0]` comments indicating:
- **ADDED** - New feature/component
- **Version 1.0.0** - Initial implementation
- Comprehensive XML documentation on all public members

## Summary

The repository pattern implementation provides:
1. ✅ Complete abstraction between business logic and data access
2. ✅ Dependency inversion through interfaces
3. ✅ Type-safe, compile-time verified dependency graph
4. ✅ Easy testing with mockable dependencies
5. ✅ Swappable implementations (SQLite → SQL Server with zero code changes)
6. ✅ Database-level filtering with IQueryable
7. ✅ Full async/await support throughout
8. ✅ Clear folder organization by responsibility
9. ✅ Comprehensive logging and error handling
10. ✅ Scalable architecture for future features

This foundation ensures the application can grow while maintaining clean architecture principles and solid coding standards.
