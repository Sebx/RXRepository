namespace App1.Repository
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using SQLite.Net;
    using SQLite.Net.Interop;
    using Interfaces;

    public class SqliteRepository<T> : IRepository<T>, IStorageRepository
        where T : class, new()
    {
        private static ReaderWriterLockSlim accessLock = new ReaderWriterLockSlim();

        private readonly ISQLitePlatform platform;

        private readonly ILocalStorage storage;

        private readonly string dbPath;

        private readonly SQLiteConnectionString connectionString;

        private SQLiteConnection dbConnection;

        private bool initialized = false;

        public SqliteRepository(ISQLitePlatform platform, ILocalStorage storage, DbPath dbpath, string dbfile = "data.db")
        {
            string dbFullPath = Path.Combine(dbpath.Path, dbfile);

            this.connectionString = new SQLiteConnectionString(dbFullPath, true, openFlags: SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache);
            this.platform = platform;
            this.storage = storage;

            this.dbPath = Path.GetDirectoryName(this.connectionString.DatabasePath);
        }

        private SQLiteConnection DbConnection
        {
            get
            {
                if (this.dbConnection == null)
                {
                    this.dbConnection = new SQLiteConnection(this.platform, this.connectionString.DatabasePath, openFlags: SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache, storeDateTimeAsTicks: true);
                    this.dbConnection.TraceListener = new DebugTraceListener();
                    this.dbConnection.ExecuteScalar<string>("PRAGMA journal_mode = WAL");
                }

                return this.dbConnection;
            }
        }

        #region " Basic CRUD functionality "

        public async Task Add(T entity)
        {
            await this.EnsureInitialized().ConfigureAwait(false);

            accessLock.TryEnterWriteLock(Timeout.Infinite);

            try
            {
                var sucess = this.DbConnection.Insert((T)entity) == 1;

                if (!sucess)
                {
                    throw new InvalidOperationException("Entity not inserted.");
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Entity not inserted." + e.Message);
            }
            finally
            {
                accessLock.ExitWriteLock();
            }
        }

        public async Task Update(T entity)
        {
            await this.EnsureInitialized().ConfigureAwait(false);

            accessLock.TryEnterWriteLock(Timeout.Infinite);

            try
            {
                var sucess = this.DbConnection.Update((T)entity, typeof(T)) == 1;

                if (!sucess)
                {
                    throw new InvalidOperationException("Entity not updated.");
                }
            }
            catch
            {
            }
            finally
            {
                accessLock.ExitWriteLock();
            }
        }

        public async Task Delete(T entity)
        {
            await this.EnsureInitialized().ConfigureAwait(false);

            accessLock.TryEnterWriteLock(Timeout.Infinite);

            try
            {
                var sucess = this.DbConnection.Delete((T)entity) == 1;

                if (!sucess)
                {
                    throw new InvalidOperationException("Entity not deleted.");
                }
            }
            catch
            {
            }
            finally
            {
                accessLock.ExitWriteLock();
            }
        }

        public async Task DeleteAll()
        {
            await this.EnsureInitialized().ConfigureAwait(false);

            accessLock.TryEnterReadLock(Timeout.Infinite);

            try
            {
                this.DbConnection.DeleteAll<T>();
            }
            catch
            {
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        public async Task<T> Get(object id)
        {
            await this.EnsureInitialized().ConfigureAwait(false);

            accessLock.TryEnterReadLock(Timeout.Infinite);

            try
            {
                return this.DbConnection.Find<T>(id);
            }
            catch
            {
                return null;
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        public async Task<IList<TEntity>> GetAll<TEntity>()
        {
            await this.EnsureInitialized().ConfigureAwait(false);

            accessLock.TryEnterReadLock(Timeout.Infinite);

            try
            {
                return (IList<TEntity>)this.DbConnection.Table<T>().ToList();
            }
            catch
            {
                return new List<TEntity>();
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        public async Task<IList<T>> GetAll(Expression<Func<T, bool>> filter)
        {
            await this.EnsureInitialized().ConfigureAwait(false);

            accessLock.TryEnterReadLock(Timeout.Infinite);

            try
            {
                return this.DbConnection.Table<T>().Where(filter).ToList();
            }
            catch
            {
                return new List<T>();
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        public async Task<int> GetCount()
        {
            await this.EnsureInitialized().ConfigureAwait(false);

            accessLock.TryEnterReadLock(Timeout.Infinite);

            try
            {
                return this.DbConnection.Table<T>().Count();
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        public async Task<IList<T>> Query(string query, params object[] args)
        {
            await this.EnsureInitialized().ConfigureAwait(false);

            accessLock.TryEnterReadLock(Timeout.Infinite);

            try
            {
                return this.DbConnection.Query<T>($"SELECT * FROM {this.DbConnection.GetMapping<T>().TableName} WHERE {query}", args);
            }
            catch
            {
                return new List<T>();
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        public async Task<bool> Delete(string where, params object[] args)
        {
            await this.EnsureInitialized().ConfigureAwait(false);

            accessLock.TryEnterReadLock(Timeout.Infinite);

            try
            {
                string query;

                query = string.Format("DELETE FROM {0} WHERE {1}", this.DbConnection.GetMapping<T>().TableName, where);

                this.DbConnection.Execute(query, args);

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        public async Task<bool> Update(string update, string where = null, params object[] args)
        {
            await this.EnsureInitialized().ConfigureAwait(false);

            accessLock.TryEnterReadLock(Timeout.Infinite);

            try
            {
                string query;

                if (string.IsNullOrEmpty(where))
                {
                    query = string.Format("UPDATE {0} SET {1}", this.DbConnection.GetMapping<T>().TableName, update);
                }
                else
                {
                    query = string.Format("UPDATE {0} SET {1} WHERE {2}", this.DbConnection.GetMapping<T>().TableName, update, where);
                }

                this.DbConnection.Execute(query, args);

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        #endregion

        #region " Metadata execution "

        private void Execute(string query, params object[] parameters)
        {
            try
            {
                accessLock.EnterReadLock();

                this.DbConnection.Execute(query, parameters);
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        public void ExecuteNonQuery(string query)
        {
            this.Execute(query);
        }

        public void ExecuteNonQuery(string query, params object[] parameters)
        {
            this.Execute(query, parameters);
        }

        public void AddTable()
        {
            try
            {
                accessLock.EnterReadLock();

                this.DbConnection.CreateTable<T>();
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        public void AddColumn(string columnName, string columnType)
        {
            string query = string.Format("ALTER TABLE {0} ADD {1} {2}", this.DbConnection.GetMapping<T>().TableName, columnName, columnType);

            this.ExecuteNonQuery(query);
        }

        public bool ExistColumn(string columnName)
        {
            try
            {
                accessLock.EnterReadLock();

                return this.DbConnection.GetTableInfo(this.DbConnection.GetMapping<T>().TableName).Where(x => x.Name == columnName).Count() > 0;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        public bool ExistColumn(string columnName, Type type)
        {
            try
            {
                bool result;

                accessLock.EnterReadLock();

                ColumnInfo columnInfo = this.GetColumn(columnName);

                if (columnInfo == null)
                {
                    result = false;
                }
                else
                {
                    Type mappedType;

                    if (ColumnInfo.SQLiteTypeMappings.TryGetValue(columnInfo.Type.ToLowerInvariant(), out mappedType))
                    {
                        result = mappedType.Equals(type);
                    }
                    else
                    {
                        result = false;
                    }
                }

                return result;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        public bool TableExists()
        {
            try
            {
                using (var cn = new SQLiteConnection(this.platform, this.connectionString.DatabasePath, openFlags: SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache, storeDateTimeAsTicks: true))
                {
                    cn.Table<T>().Count();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistDatabase()
        {
            return await this.storage.ExistFile(this.connectionString.DatabasePath).ConfigureAwait(false);
        }

        private ColumnInfo GetColumn(string columnName)
        {
            return this.GetColumns().FirstOrDefault(x => x.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        private IEnumerable<ColumnInfo> GetColumns()
        {
            return this.DbConnection.Query<ColumnInfo>($"PRAGMA table_info('{this.DbConnection.GetMapping<T>().TableName}');");
        }

        #endregion

        private async Task EnsureInitialized()
        {
            if (!this.initialized)
            {
                this.initialized = true;

                if (!await this.storage.ExistFile(this.connectionString.DatabasePath).ConfigureAwait(false))
                {
                    if (!await this.storage.ExistFolder(Path.GetDirectoryName(this.connectionString.DatabasePath)).ConfigureAwait(false))
                    {
                        await this.storage.CreateFolder(Path.GetDirectoryName(this.connectionString.DatabasePath)).ConfigureAwait(false);
                    }

                    using (var cn = new SQLiteConnection(this.platform, this.connectionString.DatabasePath, openFlags: SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache, storeDateTimeAsTicks: true))
                    {
                        var result = cn.CreateTable<T>();
                    }
                }
                else
                {
                    using (var cn = new SQLiteConnection(this.platform, this.connectionString.DatabasePath, openFlags: SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache, storeDateTimeAsTicks: true))
                    {
                        if (!this.TableExists())
                        {
                            var result = cn.CreateTable<T>();
                        }
                    }
                }
            }

            if (this.dbConnection == null)
            {
                this.dbConnection = new SQLiteConnection(this.platform, this.connectionString.DatabasePath, openFlags: SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache, storeDateTimeAsTicks: true);
                this.dbConnection.TraceListener = new DebugTraceListener();
                this.dbConnection.ExecuteScalar<string>("PRAGMA journal_mode = WAL");
            }
        }

        public void Dispose()
        {
            if (this.dbConnection != null)
            {
                this.dbConnection.Dispose();
                this.dbConnection = null;
            }
        }
    }
}
