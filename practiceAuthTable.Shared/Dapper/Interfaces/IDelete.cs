using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dapper.Interfaces
{
    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface IDelete<TEntity>
      where TEntity : class
    {
        /// <summary>
        /// Delete an existing entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Delete(TEntity entity, IDbTransaction transaction = null);
    }
}
