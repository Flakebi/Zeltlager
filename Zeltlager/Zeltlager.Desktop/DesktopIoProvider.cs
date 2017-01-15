using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	public class DesktopIoProvider : IIoProvider
	{
		public Task<Tuple<string, FileType>[]> ListContents(string path)
		{
			return Task.FromResult(
				Directory.GetFiles(path).Select(f =>
					new Tuple<string, FileType>(Path.GetFileName(f), FileType.File)
				).Concat(Directory.GetDirectories(path).Select(
					f => new Tuple<string, FileType>(Path.GetFileName(f), FileType.Folder))
				).ToArray()
			);
		}
		public Task CreateFolder(string path) => Task.FromResult(Directory.CreateDirectory(path));
		public Task<bool> ExistsFile(string path) => Task.FromResult(File.Exists(path));
		public Task<bool> ExistsFolder(string path) => Task.FromResult(Directory.Exists(path));
		public Task<Stream> ReadFile(string path) => Task.FromResult((Stream)File.Open(path, FileMode.Open));
		public Task<Stream> WriteFile(string path) => Task.FromResult((Stream)File.Open(path, FileMode.Create));
		public Task<Stream> AppendFile(string path) => Task.FromResult((Stream)File.Open(path, FileMode.Append));
	}
}
