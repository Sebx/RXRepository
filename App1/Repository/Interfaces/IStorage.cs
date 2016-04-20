using System.Threading.Tasks;

namespace App1.Repository.Interfaces
{
    public interface IStorage
    {
        void Save<T>(string key, T value) where T : class;

        T Load<T>(string key) where T : class;

        Task<string> LoadAsync(string key);

        void Remove(string key);
    }
}
