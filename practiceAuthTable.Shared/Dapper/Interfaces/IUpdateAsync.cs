using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dapper.Interfaces
{
    /// <summary>
    /// Updates an etity.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface IUpdateAsync<TEntity>
      where TEntity : class
    {
        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Task</returns>
        Task UpdateAsync(TEntity entity, IDbTransaction transaction = null);

        Task UpdateWithConcurrencyAsync(TEntity entity,IDbTransaction transaction=null);
    }
}
