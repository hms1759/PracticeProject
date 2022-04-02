using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dapper.Interfaces
{
    /// <summary>
    /// Creates a new entity.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface ICreate<TEntity>
      where TEntity : class
    {
        /// <summary>
        /// Create a new entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Create(TEntity entity, IDbTransaction transaction = null);
    }
}
