using System.Linq.Expressions;

namespace API.Abstractions.Data.Repositories;

/// <summary>
/// [ADDED|1.0.0] Generic repository interface for all data access operations
/// Provides abstraction between business logic and data layer, enabling easy switching of persistence mechanisms
/// </summary>
/// <typeparam name="T">Entity type managed by this repository</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Retrieves an entity by its primary key identifier
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The entity if found; null otherwise</returns>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities with no tracking (read-only)
    /// Uses IQueryable for lazy evaluation - filters are applied at database level
    /// </summary>
    /// <returns>IQueryable collection for further filtering</returns>
    IQueryable<T> GetAll();

    /// <summary>
    /// Finds entities matching the specified predicate with no tracking
    /// Uses lazy evaluation via IQueryable
    /// </summary>
    /// <param name="predicate">Filter expression</param>
    /// <returns>IQueryable of matching entities</returns>
    IQueryable<T> Find(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Adds a single entity to the repository
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Completed task</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities to the repository in a single operation
    /// </summary>
    /// <param name="entities">Entities to add</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Completed task</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null or empty</exception>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">Entity with updated values</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Completed task</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a single entity
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Completed task</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities in a single operation
    /// </summary>
    /// <param name="entities">Entities to delete</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Completed task</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null or empty</exception>
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specified condition
    /// Useful for existence checks and validations
    /// </summary>
    /// <param name="predicate">Filter expression</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>True if any entity matches; false otherwise</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the optional predicate
    /// If no predicate provided, returns total count
    /// </summary>
    /// <param name="predicate">Optional filter expression; null returns total count</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Number of matching entities</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes to the database
    /// Should be called after Add/Update/Delete operations
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Number of rows affected</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
