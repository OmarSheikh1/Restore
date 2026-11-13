---
description: "Use when: making code changes, writing commits, or updating code to ensure all modifications are properly documented with labels, comments, and changelog entries"
applyTo: "**/*.cs"
---

# Code Change Documentation & Labeling Standards

All code changes MUST be properly documented with labels, comments, and changelog entries. This ensures traceability, maintainability, and clear understanding of what changed and why.

## Commit Message Format - MANDATORY

Every commit MUST follow this structured format:

```
[LABEL] Brief description of the change (50 chars max)

Detailed explanation of:
- WHAT changed
- WHY it changed
- HOW it was implemented
- Any breaking changes or side effects

Fixes: #<issue-number> (if applicable)
Related: #<pr-number> (if applicable)
```

### Commit Labels

Use exactly ONE primary label per commit:

| Label | Usage | Example |
|-------|-------|---------|
| `[FEAT]` | New feature or functionality | `[FEAT] Add product filtering service` |
| `[FIX]` | Bug fix | `[FIX] Correct null reference in repository` |
| `[REFACTOR]` | Code restructuring (no behavior change) | `[REFACTOR] Extract validation logic to service` |
| `[PERF]` | Performance improvement | `[PERF] Add caching to product queries` |
| `[DOCS]` | Documentation only | `[DOCS] Update API endpoint examples` |
| `[TEST]` | Test additions/changes | `[TEST] Add unit tests for ProductService` |
| `[STYLE]` | Code style (formatting, naming) | `[STYLE] Align naming conventions with standards` |
| `[SECURITY]` | Security fix or hardening | `[SECURITY] Validate user input in controllers` |
| `[DEPS]` | Dependency updates | `[DEPS] Upgrade EntityFrameworkCore to 8.0` |
| `[CHORE]` | Build, CI/CD, tooling | `[CHORE] Update GitHub Actions workflow` |

### Commit Message Example

```
[FEAT] Implement product filtering and pagination service

WHAT:
- Added IProductService interface with filtering capabilities
- Implemented ProductService with support for:
  - Category filtering
  - Price range filtering
  - Full-text search
  - Pagination (page, pageSize)
  - Sorting (by name, price, created date)

WHY:
- Product list endpoint was not scalable for large datasets
- Clients needed flexible filtering options
- Current implementation was loading all products into memory

HOW:
- Created IProductService interface in Abstractions/Services/
- Implemented ProductService using repository pattern
- Used IQueryable for lazy evaluation of filters
- Added ProductFilterDto for query parameters
- Implemented PagedResult<T> for paginated responses

BREAKING CHANGES:
- GET /api/products endpoint now requires filter parameters
- Response format changed to include pagination metadata

Fixes: #42
Related: #38
Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

## Code Comments - MANDATORY

All code changes MUST include relevant comments explaining:

### 1. Complex Logic Comments

❌ **AVOID:**
```csharp
public async Task<IEnumerable<Product>> GetFilteredAsync(ProductFilterDto filter)
{
    var query = _repository.GetAll();
    query = query.Where(p => p.Price >= filter.MinPrice);
    query = query.Where(p => p.Price <= filter.MaxPrice);
    return await query.ToListAsync();
}
```

✅ **CORRECT:**
```csharp
public async Task<IEnumerable<Product>> GetFilteredAsync(ProductFilterDto filter)
{
    var query = _repository.GetAll();
    
    // Filter by price range using IQueryable for lazy evaluation
    // This prevents loading all products into memory
    if (filter.MinPrice.HasValue)
        query = query.Where(p => p.Price >= filter.MinPrice);
    
    if (filter.MaxPrice.HasValue)
        query = query.Where(p => p.Price <= filter.MaxPrice);
    
    return await query.ToListAsync();
}
```

### 2. Why Comments (Intent)

Document the "why" not the "what":

❌ **BAD:**
```csharp
// Add null check
if (repository == null)
    throw new ArgumentNullException(nameof(repository));
```

✅ **GOOD:**
```csharp
// Fail fast if dependency is not injected properly
// This catches configuration errors at service instantiation
if (repository == null)
    throw new ArgumentNullException(nameof(repository));
```

### 3. Change-Specific Comments

When modifying existing code, add a comment indicating what changed:

```csharp
public async Task<Product> GetByIdAsync(int id)
{
    var product = await _dbSet.FindAsync(id);
    
    // [CHANGE] Added explicit null check after refactoring repository pattern
    // Previously relied on implicit null handling, now throwing explicit exception
    if (product == null)
        throw new NotFoundException($"Product with ID {id} not found");
    
    return product;
}
```

### 4. TODO/FIXME Comments

Use structured TODO/FIXME comments:

```csharp
// TODO: [2026-11-13] Implement caching for product queries
// Reason: Current queries hit database on every request
// Ticket: #45
// Author: @OmarSheikh1

// FIXME: [2026-11-13] N+1 query problem in GetAllWithDetails
// This loads all products then details separately
// Should use .Include() instead
// Ticket: #52
```

### 5. Documentation Comments (XML)

All public classes, methods, and properties MUST have XML documentation:

```csharp
/// <summary>
/// Retrieves a product by its unique identifier with comprehensive details.
/// </summary>
/// <param name="id">The product ID to retrieve.</param>
/// <param name="cancellationToken">Token to cancel the operation.</param>
/// <returns>
/// A <see cref="ProductDto"/> if found; throws <see cref="NotFoundException"/> otherwise.
/// </returns>
/// <exception cref="ArgumentException">Thrown when ID is less than or equal to 0.</exception>
/// <exception cref="NotFoundException">Thrown when product is not found in database.</exception>
/// <remarks>
/// This method uses IQueryable filtering to avoid loading unnecessary data.
/// Includes related entities: Category, Reviews.
/// </remarks>
/// <example>
/// <code>
/// var product = await _service.GetByIdAsync(123);
/// </code>
/// </example>
public async Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
{
    // Implementation
}
```

## Code Change Label System

Add labels to modified code sections using structured comments:

### Label Format
```csharp
// [LABEL|VERSION|DATE|REASON]
```

### Standard Labels

```csharp
// [ADDED|1.2.0|2026-11-13] New filtering logic for category-based search
public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
{
    return await _repository.Find(p => p.Category == category).ToListAsync();
}

// [MODIFIED|1.1.5|2026-11-13] Changed from direct DbContext access to repository
// Previous: _context.Products.FirstOrDefault(p => p.Id == id)
// New: _repository.GetByIdAsync(id)
public async Task<Product> GetByIdAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

// [DEPRECATED|1.1.0|2026-11-13] Use GetByIdAsync instead
// This method will be removed in v2.0
[Obsolete("Use GetByIdAsync instead", false)]
public Product GetById(int id)
{
    return _repository.GetByIdAsync(id).Result;
}

// [SECURITY|1.2.1|2026-11-13] Input validation added to prevent SQL injection
if (string.IsNullOrWhiteSpace(searchTerm))
    throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));

// [PERF|1.3.0|2026-11-13] Changed to use cached results
// Previous: Queries database every time, ~200ms per request
// New: 5-minute cache, ~5ms per request
```

## Documentation Updates - MANDATORY

Every code change MUST update related documentation:

### 1. README.md Updates

When adding features:
```markdown
## Features

- ✅ Product management (CRUD)
- ✅ Advanced filtering with pagination [ADDED: v1.2.0]
- ✅ Search functionality [ADDED: v1.2.0]
- ✅ Validation framework [ADDED: v1.1.0]
```

### 2. CHANGELOG.md Updates

```markdown
## [Unreleased]

### Added
- Product filtering service with category, price range, and search support
- Pagination support for product listings
- ProductFilterDto for query parameter validation

### Changed
- GET /api/products endpoint now returns paginated results
- Product response format updated to include pagination metadata

### Fixed
- N+1 query problem in product retrieval
- Memory leak in repository disposal

### Deprecated
- ProductService.GetAllProducts() - use GetAllAsync() instead

### Security
- Added input validation for search terms
- Implemented rate limiting on API endpoints

## [1.1.0] - 2026-11-10

### Added
- Initial repository pattern implementation
- Basic CRUD operations for products
```

### 3. API Documentation

Update swagger/OpenAPI comments when changing endpoints:

```csharp
/// <summary>
/// [CHANGED: v1.2.0] Get products with advanced filtering and pagination
/// </summary>
/// <remarks>
/// Supports filtering by category, price range, and search term.
/// Results are paginated with configurable page size.
/// 
/// Change Summary:
/// - Previously returned all products as array
/// - Now returns paginated result with metadata
/// - Query parameters changed from array to object
/// </remarks>
[HttpGet]
[Produces("application/json")]
[ProduceResponseType(typeof(PagedResult<ProductDto>), 200)]
[ProduceResponseType(400)]
public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
    [FromQuery] ProductFilterDto filter,
    CancellationToken cancellationToken)
{
    // Implementation
}
```

## Pull Request Description Template

When creating PRs, use this template:

```markdown
## Description
[Describe what this PR does]

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update
- [ ] Refactoring
- [ ] Performance improvement

## Changes Made
- [ADDED] New ProductFilterDto for query parameters
- [MODIFIED] ProductService to use repository pattern
- [REMOVED] Legacy GetAllProducts method
- [DEPRECATED] ProductService.GetBySku (use repository instead)

## Related Issues
Fixes #42
Related to #38, #45

## Testing
- [x] Unit tests added
- [x] Integration tests updated
- [ ] Manual testing required

## Breaking Changes
- ⚠️ GET /api/products response format changed from array to paginated object
- ⚠️ Removed deprecated GetAllProducts method

## Files Changed
- `Abstractions/Services/IProductService.cs` - [ADDED]
- `Services/ProductService.cs` - [MODIFIED]
- `Controllers/ProductsController.cs` - [MODIFIED]
- `Data/Repositories/ProductRepository.cs` - [MODIFIED]

## Checklist
- [x] Code follows style guidelines
- [x] Self-review completed
- [x] Comments added for complex logic
- [x] Documentation updated
- [x] No breaking changes (or documented)
- [x] Unit tests pass
- [x] Commit messages follow standards
```

## GitHub Labels for Issues & PRs

Use these labels consistently:

| Label | Color | Usage |
|-------|-------|-------|
| `type: feature` | 🟢 Green | New functionality |
| `type: bug` | 🔴 Red | Bug fix |
| `type: refactor` | 🟡 Yellow | Code restructuring |
| `type: docs` | 🔵 Blue | Documentation |
| `type: test` | 🟣 Purple | Tests |
| `type: security` | 🟠 Orange | Security issues |
| `type: performance` | 🔵 Cyan | Performance |
| `priority: critical` | 🔴 Red | Blocking |
| `priority: high` | 🟠 Orange | Important |
| `priority: medium` | 🟡 Yellow | Standard |
| `priority: low` | 🟢 Green | Nice to have |
| `status: in-progress` | 🟦 Blue | Being worked on |
| `status: review` | 🟪 Purple | Waiting for review |
| `status: blocked` | 🔴 Red | Waiting for something |
| `area: api` | Blue | API changes |
| `area: database` | Green | Database changes |
| `area: ui` | Purple | UI changes |

## Code Review Comment Template

When reviewing code, use structured comments:

```markdown
### [ISSUE] Missing error handling
**File:** ProductService.cs:45
**Severity:** High

What's wrong:
```csharp
var product = await _repository.GetByIdAsync(id);
return _mapper.Map<ProductDto>(product);
```

This doesn't handle the case where product is null, causing null reference exception.

Suggested fix:
```csharp
var product = await _repository.GetByIdAsync(id);
if (product == null)
    throw new NotFoundException($"Product {id} not found");
return _mapper.Map<ProductDto>(product);
```

**Related to:** [SOLID Principles](./csharp-coding-standards.instructions.md)
---

### [IMPROVEMENT] Performance optimization
**File:** ProductService.cs:80
**Severity:** Medium

Current implementation:
```csharp
var allProducts = await _repository.GetAll().ToListAsync();
return allProducts.Where(p => p.Price > minPrice).ToList();
```

This materializes all products before filtering, causing memory issues with large datasets.

Suggested improvement:
```csharp
return await _repository.Find(p => p.Price > minPrice).ToListAsync();
```

This keeps the filtering in the database query.
```

## Interface & Implementation Change Labeling

When adding new features, label all related changes:

```csharp
// Abstractions/Services/IProductService.cs
public interface IProductService
{
    // [ADDED|1.2.0] New method for filtered product retrieval
    Task<PagedResult<ProductDto>> GetAllAsync(ProductFilterDto filter, CancellationToken cancellationToken = default);
}

// Services/ProductService.cs
public class ProductService : IProductService
{
    // [ADDED|1.2.0] Implementation of filtering with pagination
    // Related: IProductService.GetAllAsync
    public async Task<PagedResult<ProductDto>> GetAllAsync(ProductFilterDto filter, CancellationToken cancellationToken = default)
    {
        // Implementation with label comments
    }
}

// Controllers/ProductsController.cs
[HttpGet]
// [MODIFIED|1.2.0] Endpoint now uses filtered service
// Previous: Return all products in array
// New: Return paginated filtered results
public async Task<ActionResult<PagedResult<ProductDto>>> GetAll([FromQuery] ProductFilterDto filter)
{
    // Implementation
}
```

## Changelog Entry Template

Every significant code change needs a CHANGELOG entry:

```markdown
### Added
- [ProductFilterDto] New query parameter object for filtering (file: Dtos/ProductFilterDto.cs)
- [IProductService.GetAllAsync()] Filtering and pagination support (file: Abstractions/Services/IProductService.cs)
- [ProductService] Implementation with category, price, and search filters (file: Services/ProductService.cs)

### Changed
- [GET /api/products] Response format: array → PagedResult<ProductDto> with metadata
- [ProductRepository] Now inherits from generic Repository<T> base class
- [StoreContext] Added index on Product.Category for query performance

### Fixed
- [ProductService] Fixed N+1 query problem when loading products with categories
- [ProductRepository] Memory leak in DbContext disposal

### Deprecated
- [ProductService.GetAllProducts()] Use GetAllAsync() with ProductFilterDto instead

### Security
- [ProductService] Added input validation for search terms to prevent SQL injection
- [Controllers] Implemented rate limiting on all product endpoints

### Performance
- [ProductRepository] Added database index on Category column
- [ProductService] Query filtering now happens at database level, not in-memory
```

## Summary of When to Label/Comment

| Change Type | Commit Label | Code Comment | Documentation | PR Label |
|-------------|-------------|-------------|----------------|----------|
| New feature | `[FEAT]` | ✅ All methods | ✅ README + CHANGELOG | `type: feature` |
| Bug fix | `[FIX]` | ✅ Issue explanation | ✅ CHANGELOG | `type: bug` |
| Refactoring | `[REFACTOR]` | ✅ Why changed | ⚠️ If breaking | `type: refactor` |
| Perf improvement | `[PERF]` | ✅ Old vs new approach | ✅ CHANGELOG | `type: performance` |
| Security fix | `[SECURITY]` | ✅ Vulnerability detail | ✅ CHANGELOG | `type: security` |
| Dependency update | `[DEPS]` | ❌ Not needed | ⚠️ If breaking | `type: dependencies` |
| Tests | `[TEST]` | ✅ Test description | ❌ Not needed | `type: test` |
| Documentation | `[DOCS]` | ❌ Not needed | ✅ Document only | `type: docs` |

## Best Practices

1. **One change per commit** — Easy to revert and understand
2. **Descriptive commit messages** — Future you will thank you
3. **Comment the why, not the what** — Code shows what; comments explain why
4. **Keep documentation in sync** — Stale docs are worse than no docs
5. **Use labels consistently** — They're searchable and reportable
6. **Reference issues in commits** — Connects code changes to requirements
7. **Review your own comments** — Ensure they're clear and accurate
8. **Avoid commented-out code** — Use git history instead
9. **Update CHANGELOG immediately** — Not after release
10. **Link related changes** — Use issue/PR references

## Tools to Help

- **Commitizen** — Enforces commit message format
- **Husky** — Git hooks for pre-commit checks
- **GitHub Actions** — Validate commit format automatically
- **SonarQube** — Code quality and coverage reports
- **Renovate** — Automated dependency updates with proper labeling

## Example Workflow

```bash
# 1. Create feature branch
git checkout -b feat/product-filtering

# 2. Make code changes with proper comments
# [Edit files with [ADDED], [MODIFIED] labels]

# 3. Stage changes
git add .

# 4. Commit with proper label and message
git commit -m "[FEAT] Add product filtering and pagination

Add ProductFilterDto for query parameters.
Implement filtering by category, price range, and search term.
Add pagination support with configurable page size.

This resolves the N+1 query problem and improves performance.

Fixes: #42"

# 5. Create PR with template
git push origin feat/product-filtering
# Create PR on GitHub with description template

# 6. Update documentation
# Edit README.md, CHANGELOG.md, API docs

# 7. Merge after review
```

This ensures every change is properly tracked, documented, and labeled! 📝
