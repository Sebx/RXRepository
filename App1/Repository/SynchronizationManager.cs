using App1.Repository.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Windows.Foundation;
using Windows.Storage;

namespace App1.Repository
{
    public class SynchronizationManager
    {
        private IList<IMemoryRepository> context;

        private string instaceID;

        private const string synchronizationTag = "SYNCHRONIZATIONMANAGER";

        public SynchronizationManager(IList<IMemoryRepository> context)
        {
            this.instaceID = Guid.NewGuid().ToString();
            this.context = context;
            ApplicationData.Current.DataChanged += new TypedEventHandler<ApplicationData, object>(DataChangeHandler);

            foreach (var settingKV in ApplicationData.Current.LocalSettings.Values)
            {
                if (settingKV.Key.Contains(synchronizationTag))
                {
                    ApplicationData.Current.LocalSettings.Values.Remove(settingKV.Key);                    
                }
            }
        }

        void DataChangeHandler(ApplicationData appData, object o)
        {
            string actionName = null;

            object param = null;

            Type entityType = null;

            Debug.WriteLine(" DataChangeHandler ");

            foreach (var settingKV in appData.LocalSettings.Values.OrderBy((t => t.Key)))
            {
                Debug.WriteLine(" DataChangeHandler settingKV.Key: " + settingKV.Key);

                if (settingKV.Key.Contains(synchronizationTag)
                    && !settingKV.Key.Contains(instaceID))
                {
                    Debug.WriteLine(" Process ");

                    var value = (ApplicationDataCompositeValue)settingKV.Value;

                    var newItemType = Type.GetType(value["NewItemType"].ToString() + ", ClaroMusica.Model");
                    var newItem = JsonConvert.DeserializeObject(value["NewItem"].ToString(), newItemType);

                    var oldItemType = Type.GetType(value["OldItemType"].ToString() + ", ClaroMusica.Model");
                    var oldItem = JsonConvert.DeserializeObject(value["OldItem"].ToString(), oldItemType);

                    switch ((NotifyCollectionChangedAction)Enum.Parse(typeof(NotifyCollectionChangedAction), value["Action"].ToString()))
                    {
                        case NotifyCollectionChangedAction.Add:
                            actionName = "Add";
                            entityType = newItemType;
                            param = newItem;
                            break;
                        case NotifyCollectionChangedAction.Move:
                            throw new NotImplementedException();
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            actionName = "Delete";
                            entityType = oldItemType;
                            param = oldItem;
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            actionName = "Update";
                            entityType = newItemType;
                            param = newItem;
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            actionName = "DeleteAll";
                            entityType = newItemType;
                            param = newItem;
                            break;
                        default:
                            break;
                    }

                    if (!String.IsNullOrEmpty(actionName))
                    {
                        var memoryInstance = context.Where((i) => i.GetType().GetGenericArguments()[0] == entityType).FirstOrDefault();

                        Debug.WriteLine(" before: " + actionName);
                        memoryInstance.MemoryOnly = true;
                        memoryInstance.GetType().GetMethod(actionName, new Type[] { entityType }).Invoke(memoryInstance, new object[] { param });
                        memoryInstance.MemoryOnly = false;
                        Debug.WriteLine(" after: " + actionName);
                    }

                    ApplicationData.Current.LocalSettings.Values.Remove(settingKV);
                }
            }
        }

        internal void NotifyChange(Tuple<object, NotifyCollectionChangedEventArgs> changedValue)
        {
            string key = synchronizationTag + "_" + instaceID + "_" + changedValue.Item1.GetType().GetGenericArguments()[0].ToString() + "_" + DateTime.Now.Ticks.ToString();

            if (changedValue.Item2 != null)
            {
                var value = changedValue.Item2;

                var composite = new ApplicationDataCompositeValue();

                composite["Action"] = value.Action.ToString();

                if (value.NewItems != null)
                {
                    composite["NewItemType"] = value.NewItems[0].GetType().ToString();
                    composite["NewItem"] = JsonConvert.SerializeObject(value.NewItems[0]);
                }
                else
                {
                    composite["NewItemType"] = typeof(string).ToString();
                    composite["NewItem"] = JsonConvert.SerializeObject(string.Empty);
                }

                if (value.OldItems != null)
                {
                    composite["OldItemType"] = value.OldItems[0].GetType().ToString();
                    composite["OldItem"] = JsonConvert.SerializeObject(value.OldItems[0]);
                }
                else
                {
                    composite["OldItemType"] = typeof(string).ToString();
                    composite["OldItem"] = JsonConvert.SerializeObject(string.Empty);
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                {
                    ApplicationData.Current.LocalSettings.Values.Add(key, composite);
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values[key] = composite;
                }

                Debug.WriteLine(" SignalDataChanged key: " + key);
                ApplicationData.Current.SignalDataChanged();
            }
        }
    }
}
