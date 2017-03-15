using System;
using System.IO;
using System.Threading.Tasks;

using Zeltlager;

namespace UnitTests
{
	public class DiscardIoProvider : IIoProvider
	{
		static readonly Task nop = Task.WhenAll();

		public Task<Tuple<string, FileType>[]> ListContents(string path) => Task.FromResult(new Tuple<string, FileType>[0]);
		public Task CreateFolder(string path) => nop;
		public Task<bool> ExistsFile(string path) => Task.FromResult(false);
		public Task<bool> ExistsFolder(string path) => Task.FromResult(false);
		public Task<Stream> ReadFile(string path) => Task.FromResult<Stream>(new MemoryStream());
		public Task<Stream> WriteFile(string path) => Task.FromResult<Stream>(new MemoryStream());
		public Task<Stream> AppendFile(string path) => Task.FromResult<Stream>(new MemoryStream());
	}
}
