using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Abstractions
{
    /// <summary>
    /// A general CRUD repository for domain entities.
    /// Does not write directly to the database – commits are handled
    /// by the Unit of Work layer via <c>SaveChangesAsync</c>.
    /// </summary>
    /// <typeparam name="T">Domain entity type.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Returns all entities of type <typeparamref name="T"/> as a list.
        /// </summary>
        /// <param name="ct">Optional cancellation token to cancel the operation.</param>
        /// <returns>A list of entities.</returns>
        Task<List<T>> GetAllAsync(CancellationToken ct = default, params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Returns an entity by its identifier, or <c>null</c> if not found.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <param name="ct">Optional cancellation token to cancel the operation.</param>
        /// <returns>The entity if found, otherwise <c>null</c>.</returns>
        Task<T?> GetByIdAsync(int id, CancellationToken ct = default, params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Adds a new entity to the context. 
        /// The entity is not persisted until <c>SaveChangesAsync</c> is called.
        /// </summary>
        /// <param name="entity">The entity instance to add.</param>
        /// <param name="ct">Optional cancellation token to cancel the operation.</param>
        Task AddAsync(T entity, CancellationToken ct = default);

        /// <summary>
        /// Marks an entity as modified in the context. 
        /// Changes are persisted only after <c>SaveChangesAsync</c> is called.
        /// </summary>
        /// <param name="entity">The entity instance to update.</param>
        void Update(T entity);

        /// <summary>
        /// Removes an entity identified by its Id from the context.
        /// The actual deletion happens only after <c>SaveChangesAsync</c> is called.
        /// </summary>
        /// <param name="id">The identifier of the entity to remove.</param>
        void Delete(int id);
    }
}
