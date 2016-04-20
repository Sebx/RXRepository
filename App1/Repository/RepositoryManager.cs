using App1.Repository.Interfaces;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;

namespace App1.Repository
{

    public class RepositoryManager
    {
        private static SemaphoreSlim _accessSemaphore = new SemaphoreSlim(1);

        private static RepositoryManager instance;

        private IList<IMemoryRepository> memoryContext;

        private IList<IStorageRepository> storageContext;

        private SynchronizationManager synchronizationManager;

        public IList<IMemoryRepository> Context
        {
            get { return memoryContext; }
        }

        private Queue<Tuple<object, NotifyCollectionChangedEventArgs>> changedEntities;

        public RepositoryManager()
        {

        }

        public static RepositoryManager GetInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            instance = new RepositoryManager();

            return instance;
        }

        public void Initialize(Type assemblyType, bool synchronize = false)
        {
            try
            {
                if (memoryContext == null)
                {
                    memoryContext = new List<IMemoryRepository>();

                    storageContext = new List<IStorageRepository>();

                    changedEntities = new Queue<Tuple<object, NotifyCollectionChangedEventArgs>>();

                    List<Type> entities = assemblyType.GetTypeInfo().Assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IDomainEntity))).ToList();

                    foreach (Type entity in entities)
                    {
                        var genericType = typeof(BaseRepository<>);

                        Type[] typeArgs = { entity };

                        Type constructed = genericType.MakeGenericType(typeArgs);

                        var instance = (IMemoryRepository)Activator.CreateInstance(constructed);

                        instance.MemoryOnly = true;

                        Synchronize(instance, entity, typeof(SqliteRepository<>));

                        Subscribe(instance);

                        memoryContext.Add(instance);
                    }
                }
                if (synchronize)
                    synchronizationManager = new SynchronizationManager(memoryContext);
            }
            catch
            {
                throw new Exception("Repository Manager :: cannot initialize service");
            }
        }

        private void Synchronize(IMemoryRepository memoryInstance, Type memoryType, Type storageType)
        {
            try
            {
                Type[] typeArgs = { memoryType };

                Type constructed = storageType.MakeGenericType(typeArgs);

                string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "data");

                var storageInstance = (IStorageRepository)Activator.CreateInstance(constructed, new SQLitePlatformWinRT(), new LocalStorage(), DbPath.Create(dbPath), "data.db");

                StorageSynchronize(memoryInstance, memoryType, storageInstance);

                storageContext.Add(storageInstance);
            }
            catch
            {
                throw new Exception("Repository Manager :: cannot synchronize service");
            }
        }

        private void StorageSynchronize(IMemoryRepository memoryInstance, Type memoryType, IStorageRepository storageInstance)
        {
            _accessSemaphore.Wait();

            try
            {
                var entities = typeof(IStorageRepository)
                    .GetMethod("GetAll")
                    .MakeGenericMethod(memoryType)
                    .Invoke(storageInstance, null);
               
                memoryInstance.Populate(entities);
            }
            catch
            {
                throw;
            }
            finally
            {
                _accessSemaphore.Release();
            }
        }

        private void Subscribe(IMemoryRepository memoryInstance)
        {
            memoryInstance.EntitiesChanges.Subscribe(args =>
            {
                var changeObj = new Tuple<object, NotifyCollectionChangedEventArgs>(args.Sender, args.EventArgs);

                changedEntities.Enqueue(changeObj);

                if (synchronizationManager !=  null)
                {
                    synchronizationManager.NotifyChange(changeObj);
                }
            });

            memoryInstance.EntitiesChanges.Throttle(TimeSpan.FromMilliseconds(1000)).Subscribe((args) =>
            {
                while (changedEntities.Count > 0)
                {
                    var entityEventArgs = changedEntities.Dequeue();

                    string actionName = null;

                    IList param = null;

                    Type entityType = null;

                    switch (entityEventArgs.Item2.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            actionName = "Add";

                            param = new List<object>();

                            for (int i = 0; i < entityEventArgs.Item2.NewItems.Count; i++)
                            {
                                param.Add(entityEventArgs.Item2.NewItems[i]);
                            }

                            entityType = param[0].GetType();

                            break;
                        case NotifyCollectionChangedAction.Move:
                            throw new NotImplementedException();
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            actionName = "Delete";

                            param = new List<object>();

                            for (int i = 0; i < entityEventArgs.Item2.OldItems.Count; i++)
                            {
                                param.Add(entityEventArgs.Item2.OldItems[i]);
                            }

                            entityType = param[0].GetType();
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            actionName = "Update";

                            param = new List<object>();

                            for (int i = 0; i < entityEventArgs.Item2.NewItems.Count; i++)
                            {
                                param.Add(entityEventArgs.Item2.NewItems[i]);
                            }

                            entityType = param[0].GetType();
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            actionName = "DeleteAll";

                            if (entityEventArgs.Item1.GetType().GetGenericArguments()[0] != null)
                                entityType = entityEventArgs.Item1.GetType().GetGenericArguments()[0];
                            else
                                return;

                            break;
                        default:
                            break;
                    }

                    if (!String.IsNullOrEmpty(actionName))
                    {
                        var storageInstance = storageContext.Where((i) => i.GetType().GetGenericArguments()[0] == entityType).FirstOrDefault();

                        if (param != null)
                        {
                            foreach (var paramItem in param)
                            {
                                storageInstance.GetType().GetMethod(actionName, new Type[] { entityType }).Invoke(storageInstance, new object[] { paramItem });
                            }
                        }
                        else
                        {
                            storageInstance.GetType().GetMethod(actionName).Invoke(storageInstance, null);
                        }
                    }
                }
            });
        }
    }
}
