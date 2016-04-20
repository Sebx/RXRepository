using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive;

namespace App1.Repository.Interfaces
{
    public interface IMemoryRepository
    {
        bool MemoryOnly { get; set; }

        void Populate(object entities);

        IObservable<EventPattern<NotifyCollectionChangedEventArgs>> EntitiesChanges { get; set; }
    }
}
