using System;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager
{
	/// <summary>
	/// An IoProvider that executes everything relative to a root path.
	/// </summary>
	public class RootedIoProvider : IIoProvider
	{
		readonly IIoProvider ioProvider;
		readonly string root;

		public RootedIoProvider(IIoProvider ioProvider, string root)
		{
			this.ioProvider = ioProvider;
			this.root = root;
		}

		public async Task<Tuple<string, FileType>[]> ListContents(string path)
		{
			return await ioProvider.ListContents(Path.Combine(root, path));
		}

		public async Task CreateFolder(string path)
		{
			await ioProvider.CreateFolder(Path.Combine(root, path));
		}

		public async Task<bool> ExistsFile(string path)
		{
			return await ioProvider.ExistsFile(Path.Combine(root, path));
		}

		public async Task<bool> ExistsFolder(string path)
		{
			return await ioProvider.ExistsFolder(Path.Combine(root, path));
		}

		public async Task<Stream> ReadFile(string path)
		{
			return await ioProvider.ReadFile(Path.Combine(root, path));
		}

		public async Task<Stream> WriteFile(string path)
		{
			return await ioProvider.WriteFile(Path.Combine(root, path));
		}

		public async Task<Stream> AppendFile(string path)
		{
			return await ioProvider.AppendFile(Path.Combine(root, path));
		}
	}
}
