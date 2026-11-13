---
description: "Use when: writing C# code to ensure adherence to SOLID principles, dependency inversion, and clean architecture patterns"
applyTo: "**/*.cs"
---

# C# Coding Standards & Architecture

This document enforces strict adherence to **SOLID principles** and **dependency inversion** across all C# code in this repository. All new code must follow these standards.

## Core Principles

### 1. Dependency Inversion Principle (DIP) — MANDATORY

**All external dependencies must be injected through interfaces, not concrete types.**

#### ✅ Correct Pattern

```csharp
// Define interface
public interface IProductRepository
{
    Task<Product> GetByIdAsync(int id);
    Task AddAsync(Product product);
}

// Inject via constructor
public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Product> FetchProductAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}

// Register in DI container
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<ProductService>();
```

#### ❌ Anti-Patterns (FORBIDDEN)

```csharp
// ❌ Direct dependency on concrete type
public class ProductService
{
    private readonly ProductRepository _repository = new ProductRepository();
}

// ❌ Service Locator pattern
public class ProductService
{
    private readonly IServiceProvider _serviceProvider;
    
    public ProductService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void DoSomething()
    {
        var repo = _serviceProvider.GetService(typeof(ProductRepository));
    }
}

// ❌ Static dependencies
public static class ProductRepository
{
    public static Product GetById(int id) { }
}
```

### 2. SOLID Principles

#### S — Single Responsibility Principle

**A class should have one, and only one, reason to change.**

✅ **Correct:**
```csharp
// Handles user validation only
public interface IUserValidator
{
    ValidationResult Validate(User user);
}

// Handles persistence only
public interface IUserRepository
{
    Task SaveAsync(User user);
}

// Orchestrates the flow
public class UserRegistrationService
{
    private readonly IUserValidator _validator;
    private readonly IUserRepository _repository;

    public UserRegistrationService(IUserValidator validator, IUserRepository repository)
    {
        _validator = validator;
        _repository = repository;
    }

    public async Task RegisterAsync(User user)
    {
        var result = _validator.Validate(user);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        await _repository.SaveAsync(user);
    }
}
```

❌ **Incorrect:**
```csharp
// God class doing everything
public class UserService
{
    public void RegisterUser(User user)
    {
        // Validation logic
        if (user.Email == null) throw new Exception("Email required");

        // Database logic
        var connection = new SqlConnection("...");
        connection.Open();
        var cmd = new SqlCommand("INSERT INTO Users...", connection);
        cmd.ExecuteNonQuery();

        // Email logic
        var emailClient = new SmtpClient();
        emailClient.Send(new MailMessage(user.Email, "Welcome!"));

        // Logging logic
        File.AppendAllText("log.txt", "User registered");
    }
}
```

#### O — Open/Closed Principle

**Software entities should be open for extension, closed for modification.**

✅ **Correct:**
```csharp
public interface INotificationChannel
{
    Task SendAsync(string message);
}

public class EmailNotificationChannel : INotificationChannel
{
    public async Task SendAsync(string message)
    {
        // Email logic
    }
}

public class SmsNotificationChannel : INotificationChannel
{
    public async Task SendAsync(string message)
    {
        // SMS logic
    }
}

public class NotificationService
{
    private readonly IEnumerable<INotificationChannel> _channels;

    public NotificationService(IEnumerable<INotificationChannel> channels)
    {
        _channels = channels;
    }

    public async Task NotifyAllAsync(string message)
    {
        await Task.WhenAll(_channels.Select(c => c.SendAsync(message)));
    }
}

// Register all implementations
services.AddScoped<INotificationChannel, EmailNotificationChannel>();
services.AddScoped<INotificationChannel, SmsNotificationChannel>();
```

❌ **Incorrect:**
```csharp
public class NotificationService
{
    public async Task SendAsync(string message, string type)
    {
        if (type == "email")
        {
            // Email logic
        }
        else if (type == "sms")
        {
            // SMS logic
        }
        else if (type == "slack")
        {
            // Slack logic — requires modification!
        }
    }
}
```

#### L — Liskov Substitution Principle

**Derived classes must be substitutable for their base classes.**

✅ **Correct:**
```csharp
public interface IRepository<T>
{
    Task<T> GetByIdAsync(int id);
    Task AddAsync(T entity);
}

public class ProductRepository : IRepository<Product>
{
    public async Task<Product> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task AddAsync(Product entity)
    {
        _context.Products.Add(entity);
        await _context.SaveChangesAsync();
    }
}

public class UserRepository : IRepository<User>
{
    public async Task<User> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task AddAsync(User entity)
    {
        _context.Users.Add(entity);
        await _context.SaveChangesAsync();
    }
}

// Both can be used interchangeably
public class DataService<T>
{
    private readonly IRepository<T> _repository;

    public DataService(IRepository<T> repository)
    {
        _repository = repository;
    }

    public async Task<T> GetAsync(int id) => await _repository.GetByIdAsync(id);
}
```

❌ **Incorrect:**
```csharp
public class PaymentRepository : IRepository<Payment>
{
    public async Task<Payment> GetByIdAsync(int id)
    {
        throw new NotImplementedException("We don't support retrieval");
    }

    public async Task AddAsync(Payment entity)
    {
        // Only supports creation, violates interface contract
    }
}
```

#### I — Interface Segregation Principle

**Clients should not be forced to depend on interfaces they don't use.**

✅ **Correct:**
```csharp
public interface IReadable<T>
{
    Task<T> GetByIdAsync(int id);
}

public interface IWritable<T>
{
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public class ReadOnlyProductService
{
    private readonly IReadable<Product> _repository;

    public ReadOnlyProductService(IReadable<Product> repository)
    {
        _repository = repository;
    }
}

public class FullProductService
{
    private readonly IReadable<Product> _readRepository;
    private readonly IWritable<Product> _writeRepository;

    public FullProductService(IReadable<Product> readRepository, IWritable<Product> writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }
}
```

❌ **Incorrect:**
```csharp
public interface IRepository<T>
{
    Task<T> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<List<T>> GetAllAsync();
}

// Read-only service is forced to implement write methods it doesn't need
public class ReadOnlyProductService : IRepository<Product>
{
    public Task<Product> GetByIdAsync(int id) => /* ... */;
    public Task AddAsync(Product entity) => throw new NotImplementedException();
    public Task UpdateAsync(Product entity) => throw new NotImplementedException();
    public Task DeleteAsync(int id) => throw new NotImplementedException();
    public Task<List<Product>> GetAllAsync() => /* ... */;
}
```

#### D — Dependency Inversion Principle (reinforced)

**High-level modules should not depend on low-level modules. Both should depend on abstractions.**

✅ **Correct Architecture:**
```
┌─────────────────────┐
│   ProductService    │  (High-level)
└──────────┬──────────┘
           │ depends on
           ▼
┌──────────────────────┐
│  IProductRepository  │  (Abstraction)
└──────────┬───────────┘
           │ implemented by
           ▼
┌──────────────────────┐
│  ProductRepository   │  (Low-level)
└──────────────────────┘
```

## Dependency Injection Best Practices

### Lifetime Management

```csharp
// Use appropriate lifetimes
services.AddSingleton<IConfiguration>();      // Single instance for app lifetime
services.AddScoped<IUserRepository>();         // New instance per HTTP request
services.AddTransient<IEmailService>();        // New instance every time
```

### Constructor Injection Only

- Inject dependencies through constructor parameters
- Never assign defaults with `??` to avoid hiding dependencies
- Make dependencies explicit and required

```csharp
✅ public ProductService(IRepository<Product> repository)
{
    _repository = repository ?? throw new ArgumentNullException(nameof(repository));
}

❌ public ProductService(IRepository<Product> repository = null)
{
    _repository = repository ?? new ProductRepository();
}
```

### Property Injection (Avoid)

Property injection should only be used for optional dependencies with clear documentation.

```csharp
// Only if truly optional
public class Logger
{
    public IFileWriter FileWriter { get; set; }
}
```

### No Service Locator

Never use `IServiceProvider` or `GetService()` as a service locator. Always inject required services through the constructor.

## Interface Design

### Naming Conventions

- **Interfaces**: Prefix with `I` (e.g., `IProductRepository`, `IUserValidator`)
- **Implementations**: Named after responsibility (e.g., `ProductRepository`, `EmailValidator`)
- **Avoid**: Suffixing implementations with `Impl` (e.g., `ProductRepositoryImpl`)

### Interface Contracts

```csharp
// Clear, focused interfaces
public interface IProductRepository
{
    Task<Product> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}
```

## File Structure

Organize code by responsibility:

```
src/
├── API/
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings.json
├── Data/
│   ├── Repositories/
│   │   ├── IRepository.cs
│   │   ├── ProductRepository.cs
│   │   └── UserRepository.cs
│   ├── StoreContext.cs
│   └── Migrations/
├── Entities/
│   ├── Product.cs
│   └── User.cs
├── Services/
│   ├── IProductService.cs
│   └── ProductService.cs
├── Validators/
│   ├── IProductValidator.cs
│   └── ProductValidator.cs
└── Infrastructure/
    └── Logging/
```

## Common Patterns

### Repository Pattern with DIP

```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    IQueryable<T> GetAll();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task RemoveAsync(T entity);
    Task SaveChangesAsync();
}

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly StoreContext _context;

    public Repository(StoreContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<T> GetByIdAsync(int id) => await _context.Set<T>().FindAsync(id);
    
    public IQueryable<T> GetAll() => _context.Set<T>();
    
    public async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
```

### Service Layer

```csharp
public interface IProductService
{
    Task<ProductDto> GetProductAsync(int id);
    Task CreateProductAsync(CreateProductDto dto);
}

public class ProductService : IProductService
{
    private readonly IRepository<Product> _repository;
    private readonly IProductValidator _validator;

    public ProductService(IRepository<Product> repository, IProductValidator validator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task<ProductDto> GetProductAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return new ProductDto { Id = product.Id, Name = product.Name };
    }

    public async Task CreateProductAsync(CreateProductDto dto)
    {
        var validation = _validator.Validate(dto);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var product = new Product { Name = dto.Name, Price = dto.Price };
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();
    }
}
```

## Code Review Checklist

When reviewing C# code, verify:

- [ ] All external dependencies are injected via constructor
- [ ] No `new` keyword used for services/repositories (except entities)
- [ ] Each class has a single, clear responsibility
- [ ] Interfaces are segregated and focused
- [ ] Concrete implementations don't violate Liskov Substitution
- [ ] No static service methods or properties
- [ ] No service locator pattern usage
- [ ] DI container properly configured in `Program.cs`
- [ ] Null arguments throw `ArgumentNullException`
- [ ] Async methods use `async/await` correctly

## Tools & Frameworks

- **Dependency Injection**: Built-in `Microsoft.Extensions.DependencyInjection`
- **Validation**: `FluentValidation` with injected validators
- **Entity Framework**: Use repositories to abstract data access
- **Configuration**: Inject `IConfiguration` or options pattern

## References

- [Microsoft - Dependency Injection in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [Clean Architecture by Robert Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
