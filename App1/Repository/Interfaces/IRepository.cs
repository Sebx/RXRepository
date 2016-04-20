namespace App1.Repository.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IRepository<TEntity> : IExecuteCommand, IDisposable
        where TEntity : class
    {
        Task Add(TEntity item);

        Task Update(TEntity item);

        Task Delete(TEntity item);

        Task DeleteAll();

        Task<TEntity> Get(object id);

        Task<IList<T>> GetAll<T>();

        Task<int> GetCount();

        Task<IList<TEntity>> GetAll(Expression<Func<TEntity, bool>> filter);

        Task<IList<TEntity>> Query(string query, params object[] args);

        Task<bool> Update(string update, string where, params object[] args);

        Task<bool> Delete(string where, params object[] args);
    }
}