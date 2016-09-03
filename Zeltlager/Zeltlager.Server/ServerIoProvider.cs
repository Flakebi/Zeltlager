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
		public async Task CreateFolder(string path)
		{
			Directory.CreateDirectory(path);
		}

		public async Task<bool> ExistsFile(string path)
		{
			return File.Exists(path);
		}

		public async Task<bool> ExistsFolder(string path)
		{
			return Directory.Exists(path);
		}

		public async Task<BinaryReader> ReadFile(string path)
		{
			return new BinaryReader(File.Open(path, FileMode.Open));
		}

		public async Task<BinaryWriter> WriteFile(string path)
		{
			return new BinaryWriter(File.Open(path, FileMode.Create));
		}
	}
}
