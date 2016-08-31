using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PCLStorage;

namespace Zeltlager
{
	class ClientIoProvider : IIoProvider
	{
		public Task CreateFolder(string path)
		{
			return FileSystem.Current.LocalStorage.CreateFolderAsync(path, CreationCollisionOption.OpenIfExists);
		}

		public async Task<BinaryReader> ReadFile(string file)
		{
			var f = await FileSystem.Current.LocalStorage.GetFileAsync(file);
			var stream = await f.OpenAsync(FileAccess.Read);
			return new BinaryReader(stream);
		}

		public async Task<BinaryWriter> WriteFile(string file)
		{
			var f = await FileSystem.Current.LocalStorage.GetFileAsync(file);
			var stream = await f.OpenAsync(FileAccess.ReadAndWrite);
			return new BinaryWriter(stream);
		}
	}
}
