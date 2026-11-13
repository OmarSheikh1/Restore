using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Abstractions.Data.Repositories;

namespace API.Data.Repositories;

/// <summary>
/// [ADDED|1.0.0] Generic repository implementation providing base CRUD operations
/// Abstraction layer between business logic and Entity Framework DbContext
/// All filtering uses IQueryable for lazy evaluation at database level
/// </summary>
/// <typeparam name="T">Entity type managed by this repository</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly StoreContext _context;
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes repository with DbContext
    /// </summary>
    /// <param name="context">The database context</param>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    public Repository(StoreContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    /// <summary>
    /// [ADDED|1.0.0] Retrieves entity by ID with asynchronous tracking
    /// </summary>
    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// [ADDED|1.0.0] Returns IQueryable of all entities with AsNoTracking for read-only access
    /// This enables filtering at database level without materializing data
    /// </summary>
    public IQueryable<T> GetAll()
    {
        return _dbSet.AsNoTracking();
    }

    /// <summary>
    /// [ADDED|1.0.0] Finds entities matching predicate with no tracking
    /// Filters are applied at database level via LINQ provider
    /// </summary>
    public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        return _dbSet.AsNoTracking().Where(predicate);
    }

    /// <summary>
    /// [ADDED|1.0.0] Adds single entity to DbSet
    /// Changes are pending until SaveChangesAsync() is called
    /// </summary>
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// [ADDED|1.0.0] Adds multiple entities in single batch operation
    /// More efficient than adding individually
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        var entityList = entities.ToList();
        if (!entityList.Any())
            throw new ArgumentException("Entity collection cannot be empty", nameof(entities));

        await _dbSet.AddRangeAsync(entityList, cancellationToken);
    }

    /// <summary>
    /// [ADDED|1.0.0] Updates an existing entity
    /// Entity should already exist in database, use Add for new entities
    /// </summary>
    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _dbSet.Update(entity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// [ADDED|1.0.0] Deletes a single entity
    /// </summary>
    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// [ADDED|1.0.0] Deletes multiple entities in single batch operation
    /// More efficient than deleting individually
    /// </summary>
    public async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        var entityList = entities.ToList();
        if (!entityList.Any())
            throw new ArgumentException("Entity collection cannot be empty", nameof(entities));

        _dbSet.RemoveRange(entityList);
        await Task.CompletedTask;
    }

    /// <summary>
    /// [ADDED|1.0.0] Checks if any entity matches the predicate
    /// Executes at database level without materializing data
    /// </summary>
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// [ADDED|1.0.0] Counts entities matching optional predicate
    /// If no predicate, returns total count of all entities
    /// </summary>
    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// [ADDED|1.0.0] Persists all pending changes to database
    /// Call this after any Add/Update/Delete operations
    /// Executes a single database transaction for all changes
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
