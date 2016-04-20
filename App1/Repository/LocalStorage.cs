using PCLStorage;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using App1.Repository.Interfaces;

namespace App1.Repository
{
    public class LocalStorage : ILocalStorage
    {
        public async Task<Stream> ReadFile(string path)
        {
            var subpath = GetRelativePath(FileSystem.Current.LocalStorage.Path, path);

            var file = await FileSystem.Current.LocalStorage.GetFileAsync(subpath);

            if (file != null)
            {
                return await file.OpenAsync(PCLStorage.FileAccess.Read);
            }

            throw new FileNotFoundException(path);
        }

        public async Task<bool> WriteFile(string path, Stream stream)
        {
            var directory = Path.GetDirectoryName(path);

            var name = Path.GetFileName(path);

            var subfolder = GetRelativePath(FileSystem.Current.LocalStorage.Path, directory);

            var folder = await FileSystem.Current.LocalStorage.CreateFolderAsync(subfolder, CreationCollisionOption.OpenIfExists);

            var file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);

            using (var fileStream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite).ConfigureAwait(false))
            {
                stream.Position = 0;

                var buffer = new byte[1024];

                var cnt = stream.Read(buffer, 0, 1024);

                while (cnt > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, cnt);

                    cnt = stream.Read(buffer, 0, 1024);
                }
            }

            return true;
        }

        public async Task<bool> ExistFile(string path)
        {
            try
            {
                var subpath = GetRelativePath(FileSystem.Current.LocalStorage.Path, path);

                var result = await FileSystem.Current.LocalStorage.CheckExistsAsync(subpath);

                return result == ExistenceCheckResult.FileExists;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistFolder(string path)
        {
            try
            {
                var subpath = GetRelativePath(FileSystem.Current.LocalStorage.Path, path);

                var result = await FileSystem.Current.LocalStorage.CheckExistsAsync(subpath);

                return result == ExistenceCheckResult.FolderExists;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteFile(string path)
        {
            if (await ExistFile(path))
            {
                var subpath = GetRelativePath(FileSystem.Current.LocalStorage.Path, path);

                var file = await FileSystem.Current.LocalStorage.GetFileAsync(subpath);

                if (file != null)
                {
                    await file.DeleteAsync();
                    return true;
                }
            }

            return false;
        }

        public async Task<Stream> OpenCreateFile(string path)
        {
            var directory = Path.GetDirectoryName(path);
            var name = Path.GetFileName(path);

            var subfolder = GetRelativePath(FileSystem.Current.LocalStorage.Path, directory);
            var folder = await FileSystem.Current.LocalStorage.CreateFolderAsync(subfolder, CreationCollisionOption.OpenIfExists);

            var file = await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);

            return await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite);
        }

        public async Task CreateFolder(string path)
        {
            var subfolder = GetRelativePath(FileSystem.Current.LocalStorage.Path, path);
            await FileSystem.Current.LocalStorage.CreateFolderAsync(subfolder, CreationCollisionOption.OpenIfExists);
        }

        private static string GetRelativePath(string rootPath, string fullPath)
        {
            if (String.IsNullOrEmpty(rootPath) || String.IsNullOrEmpty(fullPath))
            {
                return null;
            }
            else if (fullPath.Equals(rootPath))
            {
                return string.Empty;
            }
            else if (fullPath.Contains(rootPath + "\\"))
            {
                return fullPath.Substring(rootPath.Length + 1);
            }
            else
            {
                return null;
            }
        }

        public void DeleteFileSync(string path)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteFolder(string path)
        {
            throw new NotImplementedException();
        }

        public Task<HashSet<string>> GetFiles(string folder)
        {
            throw new NotImplementedException();
        }
    }
}
