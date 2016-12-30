using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	public class ServerIoProvider : IIoProvider
	{
		public Task<Tuple<string, FileType>[]> ListContents(string path)
		{
			return Task.Run(() =>
			    Directory.GetFiles(path).Select(f => new Tuple<string, FileType>(f, FileType.File)).Concat(
				    Directory.GetDirectories(path).Select(f => new Tuple<string, FileType>(f, FileType.Folder))
				).ToArray()
			);
		}
		public Task CreateFolder(string path) => Task.Run(() => Directory.CreateDirectory(path));
		public Task<bool> ExistsFile(string path) => Task.Run(() => File.Exists(path));
		public Task<bool> ExistsFolder(string path) => Task.Run(() => Directory.Exists(path));
		public Task<Stream> ReadFile(string path) => Task.Run(() => (Stream)File.Open(path, FileMode.Open));
		public Task<Stream> WriteFile(string path) => Task.Run(() => (Stream)File.Open(path, FileMode.Create));
		public Task<Stream> AppendFile(string path) => Task.Run(() => (Stream)File.Open(path, FileMode.Append));
	}
}
