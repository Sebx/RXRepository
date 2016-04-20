using App1.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace App1.Repository
{
    public class BaseRepository<T> : IRepository<T>, IMemoryRepository
         where T : class, new()
    {
        private ObservableCollection<T> Entities = new ObservableCollection<T>();

        public IObservable<EventPattern<NotifyCollectionChangedEventArgs>> EntitiesChanges { get; set; }

        public bool MemoryOnly { get; set; }

        public BaseRepository()
        {
            this.EntitiesChanges = Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(Entities, "CollectionChanged")
                .SkipWhile((e) => MemoryOnly);
        }

        public async void Populate(object entities)
        {
            foreach (T entity in (await (Task<IList<T>>)entities))
            {
                this.Add((T)entity);
            }
        }

        #region " Basic CRUD functionality "

        public async Task Add(T item)
        {
            Entities.Add(item);
        }

        public async Task Delete(object id)
        {
            Entities.Remove(await Get(id));
        }

        public async Task Delete(T item)
        {
            Entities.Remove(item);
        }

        public async Task DeleteAll()
        {
            Entities.Clear();
        }

        public async Task<bool> Delete(string where, params object[] args)
        {
            throw new NotImplementedException();
        }

        public async Task Update(T newItem)
        {
            var oldItem = Entities.Where(e => ((IDomainEntity)e).Id == ((IDomainEntity)newItem).Id).First();
            var oldIndex = Entities.IndexOf(oldItem);
            Entities[oldIndex] = newItem;
        }

        public async Task<bool> Update(string update, string where, params object[] args)
        {
            throw new NotImplementedException();
        }

        public async Task<T> Get(object id)
        {
            return Entities.Where(e => ((IDomainEntity)e).Id == id.ToString()).FirstOrDefault();
        }

        public async Task<IList<T>> GetAll(Expression<Func<T, bool>> filter)
        {
            return Entities.Where(filter.Compile()).ToList<T>();
        }

        public async Task<IList<TEntity>> GetAll<TEntity>()
        {
            return (IList<TEntity>)Entities.ToList<T>();
        }

        public async Task<int> GetCount()
        {
            return Entities.Count<T>();
        }

        public async Task<T> Get(object id, params Expression<Func<T, object>>[] includePaths)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<T>> Query(string query, params object[] args)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Contains(object id)
        {
            return Entities.Contains(await Get(id));
        }

        #endregion

        #region " Metadata execution "

        public void AddColumn(string columnName, string columnType)
        {
            throw new NotImplementedException();
        }

        public void AddTable()
        {
            throw new NotImplementedException();
        }

        public void ExecuteNonQuery(string query)
        {
            throw new NotImplementedException();
        }

        public void ExecuteNonQuery(string query, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public bool ExistColumn(string columnName)
        {
            throw new NotImplementedException();
        }

        public bool ExistColumn(string columnName, Type type)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistDatabase()
        {
            throw new NotImplementedException();
        }

        public bool TableExists()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Entities = null;
                    EntitiesChanges = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BaseRepository() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

    }
}
