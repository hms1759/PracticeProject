using Shared.Dapper.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Dapper;
using System.Data;
using System.Data.Common;

namespace Shared.Dapper
{
    public class Service<TEntity> : IService<TEntity> where TEntity : class
    {
        public IUnitOfWork UnitOfWork { get; }
        protected readonly IDapperRepository<TEntity> _dapperRepository;
        private bool _disposed;
        protected Dictionary<String, String> _errors = new Dictionary<String, String>();
        protected List<ValidationResult> Results { get; set; } = new List<ValidationResult>();

        public Service(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
            _dapperRepository = UnitOfWork.Repository<TEntity>();
        }

        public IDbTransaction Transaction { get; set; }

        public string EntityName { get => typeof(TEntity).Name; }

        protected bool ValidateObject(TEntity entity)
        {
            return Validator.TryValidateObject(entity, new ValidationContext(entity, null, null), Results, false);
        }

        public async Task<IEnumerable<DTO>> ExecuteStoredProcedure<DTO>(string sql, DynamicParameters parameters, IDbTransaction transaction = null)
        {
            return await _dapperRepository.ExecuteStoredProcedure<DTO>(sql, parameters, transaction);
        }
        public IEnumerable<Dto> SqlQuery<Dto>(string sql, object paramaters, IDbTransaction transaction = null)
        {
            return _dapperRepository.Connection.Query<Dto>(sql, paramaters, transaction);
        }

        public DbDataReader ExecuteReader(CommandType cmdType, string cmdText)
        {
            return _dapperRepository.ExecuteReader( cmdType, cmdText);
        }
        public DbDataReader ExecuteReader(CommandType cmdType, string cmdText, DynamicParameters parameters)
        {
            return _dapperRepository.ExecuteReader(cmdType, cmdText,parameters);
        }
        public virtual string GetJsonDataById(Guid id, IDbTransaction transaction = null, JsonSerializerOptions jsonSetting = null)
        {
            var result = this.FindById(id, transaction);

            if (result == null)
                return string.Empty;

            return jsonSetting != null ? JsonSerializer.Serialize(result, jsonSetting) : JsonSerializer.Serialize(result);
        }

        public TEntity FindById(Guid id, IDbTransaction transaction = null)
        {
            return _dapperRepository.GetById(id, transaction);
        }

        public IEnumerable<TEntity> Find(string sql = null, IDictionary<string, object> parameters = null, IDbTransaction transaction = null)
        {
            return _dapperRepository.Find(sql, parameters, transaction);
        }

        protected bool IsValid<T>(T entity)
        {
            return Validator.TryValidateObject(entity, new ValidationContext(entity, null, null),
              Results, false);
        }

        public void Add(TEntity entity, IDbTransaction transaction = null)
        {
            _dapperRepository.Create(entity, transaction);
        }

        public void AddRange(IEnumerable<TEntity> entities, IDbTransaction transaction = null)
        {
            _dapperRepository.CreateMany(entities, transaction);
        }

        public void Update(TEntity entity, IDbTransaction transaction = null)
        {
            _dapperRepository.Update(entity, transaction);

        }

        public void Delete(TEntity entity, IDbTransaction transaction = null)
        {
            _dapperRepository.Delete(entity, transaction);
        }

        public async Task AddAsync(TEntity entity, IDbTransaction transaction = null)
        {
            await _dapperRepository.CreateAsync(entity, transaction);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, IDbTransaction transaction = null)
        {
            await _dapperRepository.CreateManyAsync(entities, transaction);
        }

        public async Task UpdateAsync(TEntity entity, IDbTransaction transaction = null)
        {
            await _dapperRepository.UpdateAsync(entity, transaction);
        }

        public async Task UpdateWithConcurrencyAsync(TEntity entity, IDbTransaction transaction = null)
        {
            await _dapperRepository.UpdateWithConcurrencyAsync(entity, transaction);
        }

        public async Task DeleteAsync(TEntity entity, IDbTransaction transaction = null)
        {
            await _dapperRepository.DeleteAsync(entity, transaction);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                UnitOfWork.Dispose();
            }
            _disposed = true;
        }

        public string[] Errors
        {
            get
            {
                if (Results.Any())
                {
                    return Results.Select(e => e.ErrorMessage).ToArray();
                }
                return Array.Empty<string>();
            }
        }

        public string this[string columnName]
        {
            get
            {
                if (_errors.ContainsKey(columnName))
                {
                    return _errors[columnName];
                }

                return String.Empty;
            }
        }

        public Boolean HasError
        {
            get
            {
                if (Results.Any())
                {
                    return true;
                }

                return false;
            }
        }

       
    }
}
