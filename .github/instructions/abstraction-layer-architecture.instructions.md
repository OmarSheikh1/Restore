---
description: "Use when: designing data access, service layers, or repository patterns to ensure strict abstraction layers with interface segregation and dependency injection"
applyTo: "**/*.cs"
---

# Abstraction Layer Architecture

This document enforces strict abstraction layer design across all data access, business logic, and service implementations. Every implementation must have a corresponding interface, properly organized in the project structure.

## Core Architecture Principle

**Every concrete implementation MUST have an interface. Interfaces and implementations MUST be in separate folders. All dependencies MUST be injected.**

```
Abstraction Layer (Interfaces)
         ↓ (depends on)
         ↓ (never upward)
Implementation Layer (Services/Repositories)
         ↓ (depends on)
         ↓ (never upward)
Data Layer (DbContext, Repositories)
```

## Folder Structure - MANDATORY

Organize your project with explicit separation:

```
API/
├── Abstractions/                    ← All interfaces ONLY
│   ├── Data/
│   │   ├── Repositories/
│   │   │   ├── IRepository.cs
│   │   │   ├── IProductRepository.cs
│   │   │   └── IUserRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Services/
│   │   ├── IProductService.cs
│   │   ├── IUserService.cs
│   │   └── INotificationService.cs
│   └── Validators/
│       ├── IProductValidator.cs
│       └── IUserValidator.cs
│
├── Data/                            ← Implementation layer
│   ├── DbContext/
│   │   └── StoreContext.cs
│   ├── Repositories/
│   │   ├── Repository.cs            ← Generic base
│   │   ├── ProductRepository.cs
│   │   └── UserRepository.cs
│   ├── UnitOfWork.cs
│   └── Migrations/
│
├── Services/                        ← Business logic implementation
│   ├── ProductService.cs
│   ├── UserService.cs
│   └── NotificationService.cs
│
├── Validators/                      ← Validation logic implementation
│   ├── ProductValidator.cs
│   └── UserValidator.cs
│
├── Entities/                        ← Domain models (no business logic)
│   ├── Product.cs
│   ├── User.cs
│   └── BaseEntity.cs
│
└── Controllers/                     ← HTTP endpoints only
    ├── ProductsController.cs
    └── UsersController.cs
```

## Repository Pattern with Abstraction

### 1. Generic Repository Interface

**File:** `Abstractions/Data/Repositories/IRepository.cs`

```csharp
namespace API.Abstractions.Data.Repositories;

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    IQueryable<T> GetAll();
    IQueryable<T> Find(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### 2. Entity-Specific Repository Interface

**File:** `Abstractions/Data/Repositories/IProductRepository.cs`

```csharp
namespace API.Abstractions.Data.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<bool> IsSkuUniqueAsync(string sku, int excludeId = 0, CancellationToken cancellationToken = default);
}
```

### 3. Generic Repository Implementation

**File:** `Data/Repositories/Repository.cs`

```csharp
namespace API.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly StoreContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(StoreContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    public async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    public IQueryable<T> GetAll()
    {
        return _dbSet.AsNoTracking();
    }

    public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.AsNoTracking().Where(predicate);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentNullException(nameof(entities));

        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _dbSet.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentNullException(nameof(entities));

        _dbSet.RemoveRange(entities);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null 
            ? await _dbSet.CountAsync(cancellationToken) 
            : await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### 4. Entity-Specific Repository Implementation

**File:** `Data/Repositories/ProductRepository.cs`

```csharp
namespace API.Data.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(StoreContext context) : base(context)
    {
    }

    public async Task<Product> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));

        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be empty", nameof(category));

        return await _dbSet.AsNoTracking()
            .Where(p => p.Category == category)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, int excludeId = 0, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));

        var exists = await _dbSet.AnyAsync(
            p => p.Sku == sku && p.Id != excludeId, 
            cancellationToken);
        
        return !exists;
    }
}
```

## Service Layer with Filtering & Querying

Services MUST use repository abstractions. NEVER directly instantiate repositories or use DbContext.

### 1. Service Interface

**File:** `Abstractions/Services/IProductService.cs`

```csharp
namespace API.Abstractions.Services;

public interface IProductService
{
    Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetAllAsync(ProductFilterDto filter, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
```

### 2. Service Implementation

**File:** `Services/ProductService.cs`

```csharp
namespace API.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IProductValidator _validator;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        IProductValidator validator,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be greater than 0", nameof(id));

        var product = await _repository.GetByIdAsync(id, cancellationToken);
        
        if (product == null)
            throw new NotFoundException($"Product with ID {id} not found");

        _logger.LogInformation("Retrieved product: {ProductId}", id);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<PagedResult<ProductDto>> GetAllAsync(ProductFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _repository.GetAll();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(p => p.Category == filter.Category);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(filter.SearchTerm) || p.Description.Contains(filter.SearchTerm));
        }

        // Apply sorting
        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        // Count total
        var total = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<ProductDto>>(items);

        _logger.LogInformation("Retrieved {Count} products with filters", items.Count);
        return new PagedResult<ProductDto>(dtos, total, filter.Page, filter.PageSize);
    }

    public async Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));

        var products = await _repository.Find(
            p => p.Name.Contains(searchTerm) || p.Sku.Contains(searchTerm)
        ).ToListAsync(cancellationToken);

        _logger.LogInformation("Search performed for: {SearchTerm}, found: {Count}", searchTerm, products.Count);
        return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        // Validate
        var validationResult = _validator.Validate(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Check uniqueness via repository
        var skuExists = await _repository.ExistsAsync(
            p => p.Sku == dto.Sku,
            cancellationToken);

        if (skuExists)
            throw new DuplicateException($"Product with SKU '{dto.Sku}' already exists");

        var product = _mapper.Map<Product>(dto);
        await _repository.AddAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created: {ProductId}", product.Id);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product == null)
            throw new NotFoundException($"Product with ID {id} not found");

        _mapper.Map(dto, product);
        await _repository.UpdateAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product updated: {ProductId}", id);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product == null)
            throw new NotFoundException($"Product with ID {id} not found");

        await _repository.DeleteAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product deleted: {ProductId}", id);
    }

    private IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy, string sortDirection)
    {
        var isDescending = sortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false;

        return (sortBy?.ToLower()) switch
        {
            "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "created" => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Id)
        };
    }
}
```

## Unit of Work Pattern (Optional but Recommended)

For transactions across multiple repositories:

### Unit of Work Interface

**File:** `Abstractions/Data/IUnitOfWork.cs`

```csharp
namespace API.Abstractions.Data;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

### Unit of Work Implementation

**File:** `Data/UnitOfWork.cs`

```csharp
namespace API.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly StoreContext _context;
    private IProductRepository _productRepository;
    private IUserRepository _userRepository;

    public UnitOfWork(StoreContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IProductRepository Products => _productRepository ??= new ProductRepository(_context);
    public IUserRepository Users => _userRepository ??= new UserRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync();
            await _context.Database.CommitTransactionAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.RollbackTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
```

## Dependency Injection Configuration

**File:** `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register Unit of Work (if using)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();

// Register validators
builder.Services.AddScoped<IProductValidator, ProductValidator>();
builder.Services.AddScoped<IUserValidator, UserValidator>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add logging
builder.Services.AddLogging();

// Add controllers
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
```

## Controller Layer - Minimal Logic

Controllers MUST ONLY orchestrate calls to services. NO business logic in controllers.

```csharp
namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id, cancellationToken);
            return Ok(product);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Product not found: {Id}", id);
            return NotFound(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
        [FromQuery] ProductFilterDto filter,
        CancellationToken cancellationToken)
    {
        var result = await _productService.GetAllAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DuplicateException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
```

## Key Rules - MANDATORY

1. **No direct DbContext in services** — Use repositories
2. **No `new` keyword for services/repositories** — Inject via constructor
3. **No static methods in services** — Inject everything
4. **All implementations must have interfaces** — No exceptions
5. **Interfaces in `Abstractions/` folder** — Separate from implementations
6. **Implementations in feature folders** — `Data/`, `Services/`, `Validators/`
7. **Use IQueryable for filtering** — Don't materialize data prematurely
8. **Use CancellationToken throughout** — For async graceful shutdown
9. **Validate inputs in services** — Not in repositories
10. **Log operations in services** — Track business logic execution

## Query Building Guidelines

✅ **Correct:**
```csharp
public async Task<IEnumerable<Product>> GetFilteredAsync(ProductFilterDto filter)
{
    var query = _repository.GetAll();
    
    if (filter.MinPrice.HasValue)
        query = query.Where(p => p.Price >= filter.MinPrice);
    
    if (!string.IsNullOrEmpty(filter.Category))
        query = query.Where(p => p.Category == filter.Category);
    
    return await query.ToListAsync();
}
```

❌ **Incorrect:**
```csharp
public async Task<IEnumerable<Product>> GetFilteredAsync(ProductFilterDto filter)
{
    var products = await _repository.GetAll().ToListAsync();
    return products
        .Where(p => filter.MinPrice == null || p.Price >= filter.MinPrice)
        .Where(p => string.IsNullOrEmpty(filter.Category) || p.Category == filter.Category)
        .ToList();
}
```

## Testing Benefits

This architecture enables proper unit testing:

```csharp
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IProductValidator> _validatorMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _validatorMock = new Mock<IProductValidator>();
        _service = new ProductService(_repositoryMock.Object, _validatorMock.Object, null, null);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        _repositoryMock.Verify(r => r.GetByIdAsync(1, default), Times.Once);
    }
}
```

## Summary

This architecture ensures:
- ✅ Complete abstraction of data access
- ✅ Easy swapping of implementations (e.g., SQL → MongoDB)
- ✅ Full testability with mocked dependencies
- ✅ Clear separation of concerns
- ✅ Strict adherence to SOLID principles
- ✅ Type-safe, compile-time verified dependency graph
