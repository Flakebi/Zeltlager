using System.IO;
using System.Threading.Tasks;

using PCLStorage;

namespace Zeltlager.Client
{
	class IoProvider : IIoProvider
	{
		public Task CreateFolder(string path)
		{
			return FileSystem.Current.LocalStorage.CreateFolderAsync(path, CreationCollisionOption.OpenIfExists);
		}

		public async Task<bool> ExistsFile(string path)
		{
			var existance = await FileSystem.Current.LocalStorage.CheckExistsAsync(path);
			return existance == ExistenceCheckResult.FileExists;
		}

		public async Task<bool> ExistsFolder(string path)
		{
			var existance = await FileSystem.Current.LocalStorage.CheckExistsAsync(path);
			return existance == ExistenceCheckResult.FolderExists;
		}

		public async Task<Stream> WriteFile(string path)
		{
			var directory = Path.GetDirectoryName(path);
			IFile file;
			if (!string.IsNullOrEmpty(directory))
			{
				var folder = await FileSystem.Current.LocalStorage.GetFolderAsync(directory);
				file = await folder.CreateFileAsync(Path.GetFileName(path), CreationCollisionOption.ReplaceExisting);
			}
			else
				file = await FileSystem.Current.LocalStorage.CreateFileAsync(Path.GetFileName(path), CreationCollisionOption.ReplaceExisting);

			var stream = await file.OpenAsync(FileAccess.ReadAndWrite);
			return stream;
		}

		public async Task<Stream> AppendFile(string path)
		{
			if (!await ExistsFile(path))
				return await WriteFile(path);

			var directory = Path.GetDirectoryName(path);
			IFile file;
			if (!string.IsNullOrEmpty(directory))
			{
				var folder = await FileSystem.Current.LocalStorage.GetFolderAsync(directory);
				file = await folder.GetFileAsync(Path.GetFileName(path));
			}
			else
				file = await FileSystem.Current.LocalStorage.GetFileAsync(Path.GetFileName(path));

			var stream = await file.OpenAsync(FileAccess.ReadAndWrite);
			stream.Seek(0, SeekOrigin.End);
			return stream;
		}

		public async Task<Stream> ReadFile(string path)
		{
			var f = await FileSystem.Current.LocalStorage.GetFileAsync(path);
			var stream = await f.OpenAsync(FileAccess.Read);
			return stream;
		}
	}
}
