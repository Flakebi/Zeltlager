using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using PCLStorage;

namespace Zeltlager.Client
{
	class IoProvider : IIoProvider
	{
		private static string[] GetParts(string path)
		{
			List<string> parts = new List<string>();
			string s = path;
			if (!string.IsNullOrEmpty(s))
			{
				parts.Add(Path.GetFileName(s));
				while (!string.IsNullOrEmpty(s = Path.GetDirectoryName(s)))
					parts.Add(Path.GetFileName(s));
			}
			return parts.Where(p => !string.IsNullOrEmpty(p)).Reverse().ToArray();
		}

		public async Task<Tuple<string, FileType>[]> ListContents(string path)
		{
			IFolder folder = FileSystem.Current.LocalStorage;
			foreach (var p in GetParts(path))
				folder = await folder.GetFolderAsync(p);
			var files = await folder.GetFilesAsync();
			var folders = await folder.GetFoldersAsync();
			return files.Select(f => new Tuple<string, FileType>(f.Name, FileType.File))
				.Concat(folders.Select(f => new Tuple<string, FileType>(f.Name, FileType.Folder))).ToArray();
		}

		public async Task CreateFolder(string path)
		{
			IFolder folder = FileSystem.Current.LocalStorage;
			foreach (var p in GetParts(path))
			{
				// Create the subfolder if neccessary
				await folder.CreateFolderAsync(p, CreationCollisionOption.OpenIfExists);
				folder = await folder.GetFolderAsync(p);
			}
		}

		public async Task<bool> ExistsFile(string path)
		{
			IFolder folder = FileSystem.Current.LocalStorage;
			var parts = GetParts(path);
			foreach (var p in parts.Take(parts.Length - 1))
			{
				if (await folder.CheckExistsAsync(path) != ExistenceCheckResult.FolderExists)
					return false;
				folder = await folder.GetFolderAsync(p);
			}
			return await folder.CheckExistsAsync(path) == ExistenceCheckResult.FileExists;
		}

		public async Task<bool> ExistsFolder(string path)
		{
			IFolder folder = FileSystem.Current.LocalStorage;
			foreach (var p in GetParts(path))
			{
				if (await folder.CheckExistsAsync(path) != ExistenceCheckResult.FolderExists)
					return false;
				folder = await folder.GetFolderAsync(p);
			}
			return true;
		}

		public async Task<IFolder> GetParentFolder(string path)
		{
			IFolder folder = FileSystem.Current.LocalStorage;
			var parts = GetParts(path);
			foreach (var p in parts.Take(parts.Length - 1))
			{
				folder = await folder.GetFolderAsync(p);
			}
			return folder;
		}

		public async Task<Stream> WriteFile(string path)
		{
			var folder = await GetParentFolder(path);
			IFile file = await folder.CreateFileAsync(Path.GetFileName(path), CreationCollisionOption.ReplaceExisting);

			var stream = await file.OpenAsync(FileAccess.ReadAndWrite);
			return stream;
		}

		public async Task<Stream> AppendFile(string path)
		{
			if (!await ExistsFile(path))
				return await WriteFile(path);

			var folder = await GetParentFolder(path);
			IFile file = await folder.GetFileAsync(Path.GetFileName(path));

			var stream = await file.OpenAsync(FileAccess.ReadAndWrite);
			stream.Seek(0, SeekOrigin.End);
			return stream;
		}

		public async Task<Stream> ReadFile(string path)
		{
			var folder = await GetParentFolder(path);
			IFile file = await folder.GetFileAsync(Path.GetFileName(path));
			var stream = await file.OpenAsync(FileAccess.Read);
			return stream;
		}
	}
}
