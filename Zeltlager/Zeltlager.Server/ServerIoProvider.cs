using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager
{
	public class ServerIoProvider : IIoProvider
	{
		public async Task CreateFolder(string path) => Directory.CreateDirectory(path);
		public async Task<bool> ExistsFile(string path) => File.Exists(path);
		public async Task<bool> ExistsFolder(string path) => Directory.Exists(path);
		public async Task<Stream> ReadFile(string path) => File.Open(path, FileMode.Open);
		public async Task<Stream> WriteFile(string path) => File.Open(path, FileMode.Create);
		public async Task<Stream> AppendFile(string path) => File.Open(path, FileMode.Append);
	}
}
