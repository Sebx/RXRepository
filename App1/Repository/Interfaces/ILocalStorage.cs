namespace App1.Repository.Interfaces
{
    using System.IO;
    using System.Threading.Tasks;

    public interface ILocalStorage
    {
        Task<Stream> ReadFile(string path);

        Task<bool> WriteFile(string path, Stream stream);
        
        Task<bool> ExistFile(string path);
        
        Task<bool> ExistFolder(string path);
        
        Task<bool> DeleteFile(string path);

        void DeleteFileSync(string path);

        Task<bool> DeleteFolder(string path);
        
        Task<Stream> OpenCreateFile(string path);

        Task<System.Collections.Generic.HashSet<string>> GetFiles(string folder);

        Task CreateFolder(string path);
    }
}
