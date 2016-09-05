﻿using System.IO;
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

		public async Task<BinaryReader> ReadFile(string path)
		{
			var f = await FileSystem.Current.LocalStorage.GetFileAsync(path);
			var stream = await f.OpenAsync(FileAccess.Read);
			return new BinaryReader(stream);
		}

		public async Task<BinaryWriter> WriteFile(string path)
		{
			var folder = await FileSystem.Current.LocalStorage.GetFolderAsync(Path.GetDirectoryName(path));
			var f = await folder.CreateFileAsync(Path.GetFileName(path), CreationCollisionOption.ReplaceExisting);
			var stream = await f.OpenAsync(FileAccess.ReadAndWrite);
			return new BinaryWriter(stream);
		}
	}
}