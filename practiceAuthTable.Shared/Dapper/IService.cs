using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Dapper.Interfaces;
using Dapper;
using System.Data;
using System.Text.Json;

namespace Shared.Dapper
{
    public interface IService
    {
        string EntityName { get; }
        string GetJsonDataById(Guid id, IDbTransaction transaction = null, JsonSerializerOptions jsonOptions = null);
    }

    public interface IService<TEntity> : IService where TEntity : class
    {
        IDbTransaction Transaction { get; set; }
        string this[string columnName] { get; }
        string[] Errors { get; }
        bool HasError { get; }
        IUnitOfWork UnitOfWork { get; }
        void Add(TEntity entity, IDbTransaction transaction = null);
        Task AddAsync(TEntity entity, IDbTransaction transaction = null);
        void AddRange(IEnumerable<TEntity> entities, IDbTransaction transaction = null);
        Task AddRangeAsync(IEnumerable<TEntity> entities, IDbTransaction transaction = null);
        void Delete(TEntity entity, IDbTransaction transaction = null);
        Task DeleteAsync(TEntity entity, IDbTransaction transaction = null);
        void Dispose();
        void Dispose(bool disposing);
        IEnumerable<TEntity> Find(string sql = null, IDictionary<string, object> parameters = null, IDbTransaction transaction = null);
        Task<IEnumerable<DTO>> ExecuteStoredProcedure<DTO>(string sql, DynamicParameters parameters, IDbTransaction transaction = null);
        TEntity FindById(Guid id, IDbTransaction transaction = null);
        IEnumerable<Dto> SqlQuery<Dto>(string sql, object paramaters, IDbTransaction transaction = null);
        void Update(TEntity entity, IDbTransaction transaction = null);
        Task UpdateAsync(TEntity entity, IDbTransaction transaction = null);
        Task UpdateWithConcurrencyAsync(TEntity entity, IDbTransaction transaction = null);
    }
}