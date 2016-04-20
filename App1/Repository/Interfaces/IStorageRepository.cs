using System.Collections.Generic;
using System.Threading.Tasks;

namespace App1.Repository.Interfaces
{
    interface IStorageRepository : IExecuteCommand
    {
        Task<IList<T>> GetAll<T>();
    }
}
