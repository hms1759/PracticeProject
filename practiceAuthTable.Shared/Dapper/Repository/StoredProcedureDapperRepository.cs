using Shared.Dapper.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Repository
{
    /// <summary>
    /// A Dapper repository using stored procedures
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public class StoredProcedureDapperRepository<TEntity> :
      GenericRepositoryBase<TEntity>,
      IStoredProcedureDapperRepository<TEntity>
      where TEntity : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StoredProcedureDapperRepository{TEntity}"/> class
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="idProperty">Id property expression</param>
        public StoredProcedureDapperRepository(
          IDbConnection connection,
          Expression<Func<TEntity, object>> idProperty = null)
          : base(idProperty)
        {
            Connection = connection;
        }

        /// <summary>
        /// Gets parameters
        /// </summary>
        public IDictionary<string, object> Parameters { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets database connection
        /// </summary>
        protected virtual IDbConnection Connection { get; private set; }

        /// <summary>
        /// Clear parameters
        /// </summary>
        /// <returns>Current instance</returns>
        IParameterizedRepository<TEntity> IParameterizedRepository<TEntity>.ClearParameters()
        {
            return ClearParameters();
        }

        /// <summary>
        /// Clear parameters
        /// </summary>
        /// <returns>Current instance</returns>
        public IStoredProcedureDapperRepository<TEntity> ClearParameters()
        {
            Parameters.Clear();
            return this;
        }

        /// <summary>
        /// Create a new entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Create(TEntity entity, IDbTransaction transaction = null)
        {
            CreateAsync(entity, transaction).WaitSync();
        }

        /// <summary>
        /// Create a new entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Task</returns>
        public virtual async Task CreateAsync(TEntity entity, IDbTransaction transaction = null)
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            var parameters = EntityColumns
              .Where(c => c != IdPropertyName)
              .Select(c => $"@{c}=@{c}").ToList();

            var createCommand = $"EXEC Create{EntityTypeName} {string.Join(",", parameters)}";

            IEnumerable<int> result = await Connection.QueryAsync<int>(
              createCommand,
              entity, transaction);

            EntityType.GetProperty(IdPropertyName)?
              .SetValue(entity, result.First());
        }

        public virtual async Task CreateRoleClaimAsync(TEntity entity, IDbTransaction transaction = null)
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            var parameters = EntityColumns
                .Where(c => c != IdPropertyName)
                .Select(c => $"@{c}=@{c}").ToList();

            var createCommand = $"EXEC Create{EntityTypeName} {string.Join(",", parameters)}";

            IEnumerable<int> result = await Connection.QueryAsync<int>(createCommand, entity, transaction);

            EntityType.GetProperty(IdPropertyName)?
                .SetValue(entity, result.First());
        }

        /// <summary>
        /// Create a list of new entities
        /// </summary>
        /// <param name="entities">List of entities</param>
        public virtual void CreateMany(IEnumerable<TEntity> entities, IDbTransaction transaction = null)
        {
            CreateManyAsync(entities, transaction).WaitSync();
        }

        /// <summary>
        /// Create a list of new entities
        /// </summary>
        /// <param name="entities">List of entities</param>
        /// <returns>Task</returns>
        public virtual async Task CreateManyAsync(IEnumerable<TEntity> entities, IDbTransaction transaction = null)
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            var parameters = EntityColumns
              .Where(c => c != IdPropertyName)
              .Select(c => "@" + c).ToList();

            await Connection.ExecuteAsync($"EXEC Create{EntityTypeName} {string.Join(",", parameters)}", entities, transaction);
        }

        /// <summary>
        /// Delete an existing entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Delete(TEntity entity, IDbTransaction transaction = null)
        {
            DeleteAsync(entity, transaction).WaitSync();
        }

        /// <summary>
        /// Delete an existing entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Task</returns>
        public virtual async Task DeleteAsync(TEntity entity, IDbTransaction transaction = null)
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            await Connection.ExecuteAsync($"EXEC Delete{EntityTypeName} @{IdPropertyName}", entity, transaction);
        }

        /// <summary>
        /// Delete a list of existing entities
        /// </summary>
        /// <param name="entities">Entity list</param>
        public virtual void DeleteMany(IEnumerable<TEntity> entities, IDbTransaction transaction = null)
        {
            DeleteManyAsync(entities, transaction).WaitSync();
        }

        /// <summary>
        /// Delete a list of existing entities
        /// </summary>
        /// <param name="entities">Entity list</param>
        /// <returns>Task</returns>
        public virtual async Task DeleteManyAsync(IEnumerable<TEntity> entities, IDbTransaction transaction = null)
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            await Connection.ExecuteAsync($@"EXEC Delete{EntityTypeName} @{IdPropertyName}", entities, transaction);
        }

        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Update(TEntity entity, IDbTransaction transaction = null)
        {
            UpdateAsync(entity, transaction).WaitSync();
        }

        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Task</returns>
        public virtual async Task UpdateAsync(TEntity entity, IDbTransaction transaction = null)
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            var parameters = EntityColumns.Select(name => $"@{name}=@{name}").ToList();

            await Connection.ExecuteAsync($"EXEC Update {EntityTypeName} {string.Join(",", parameters)}", entity, transaction);
        }

        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Task</returns>
        public virtual async Task UpdateWithConcurrencyAsync(TEntity entity, IDbTransaction transaction)
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            var parameters = EntityColumns.Select(name => $"@{name}=@{name}").ToList();

            var rowCount = await Connection.ExecuteAsync($@"EXEC Update{EntityTypeName} {string.Join(",", parameters)}", entity, transaction);

            if (rowCount == 0)
            {
                throw new DBConcurrencyException("The entity you were trying to edit has changed. Reload the entity and try again.");
            }
        }

        /// <summary>
        /// Gets an entity by id.
        /// </summary>
        /// <param name="id">Filter</param>
        /// <returns>Entity</returns>
        public virtual TEntity GetById(object id, IDbTransaction transaction = null)
        {
            var task = GetByIdAsync(id, transaction);
            task.WaitSync();
            return task.Result;
        }

        /// <summary>
        /// Gets an entity by id.
        /// </summary>
        /// <param name="id">Filter to find a single item</param>
        /// <returns>Entity</returns>
        public virtual async Task<TEntity> GetByIdAsync(object id, IDbTransaction transaction = null)
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            IEnumerable<TEntity> result = await Connection.QueryAsync<TEntity>($"EXEC Get{EntityTypeName} @{IdPropertyName}",
              new { Id = id }, transaction);

            return result.FirstOrDefault();
        }

        public virtual IEnumerable<TEntity> GetLById(object id, IDbTransaction transaction = null)
        {
            var task = GetByLIdAsync(id, transaction);
            task.WaitSync();
            return task.Result;
        }

        public virtual async Task<IEnumerable<TEntity>> GetByLIdAsync(object id, IDbTransaction transaction = null)
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            IEnumerable<TEntity> result = await Connection.QueryAsync<TEntity>($"EXEC Get{EntityTypeName} @{IdPropertyName}",
                new { Id = id }, transaction);

            return result;
        }
        /// <summary>
        /// Get a list of entities
        /// </summary>
        /// <returns>Query result</returns>
        public virtual IEnumerable<TEntity> Find(IDbTransaction transaction = null)
        {
            var task = FindAsync(transaction);
            task.WaitSync();
            return task.Result;
        }

        /// <summary>
        /// Get a list of entities
        /// </summary>
        /// <returns>Query result</returns>
        public virtual async Task<IEnumerable<TEntity>> FindAsync(IDbTransaction transaction = null)
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            var parameters = Parameters.Keys
              .Select(key => $"@{key}=@{key}");

            return await Connection.QueryAsync<TEntity>(
              $"EXEC Find{EntityTypeName} {string.Join(",", parameters)}",
              ToObject(Parameters), transaction);
        }

        public virtual async Task<IEnumerable<TEntity>> FindEmailAsync(string email, IDbTransaction transaction = null)
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            IEnumerable<TEntity> result = await Connection.QueryAsync<TEntity>($"EXEC Get{EntityTypeName} @{IdPropertyName}",
                new { Email = email }, transaction);

            return result;
        }

        /// <summary>
        /// Gets parameter value
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns>Parameter value</returns>
        public object GetParameter(string name)
        {
            return Parameters[name];
        }

        /// <summary>
        /// Adds a parameter to queries
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <returns>Current instance</returns>
        IParameterizedRepository<TEntity> IParameterizedRepository<TEntity>.SetParameter(string name, object value)
        {
            return SetParameter(name, value);
        }

        /// <summary>
        /// Adds a parameter to queries
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <returns>Current instance</returns>
        public IStoredProcedureDapperRepository<TEntity> SetParameter(string name, object value)
        {
            if (!Parameters.Keys.Contains(name))
            {
                Parameters.Add(name, value);
            }
            else
            {
                Parameters[name] = value;
            }

            return this;
        }

        public Task CreateRolesClaimAsync(TEntity entity, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }
    }
}
