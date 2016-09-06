using System.IO;
using System.Threading.Tasks;

namespace Zeltlager
{
	/// <summary>
	/// An IoProvider that executes everything relative to a root path.
	/// </summary>
	public class RootedIoProvider : IIoProvider
	{
		IIoProvider ioProvider;
		string root;

		public RootedIoProvider(IIoProvider ioProvider, string root)
		{
			this.ioProvider = ioProvider;
			this.root = root;
		}

		public Task CreateFolder(string path) => ioProvider.CreateFolder(Path.Combine(root, path));

		public Task<bool> ExistsFile(string path) => ioProvider.ExistsFile(Path.Combine(root, path));

		public Task<bool> ExistsFolder(string path) => ioProvider.ExistsFolder(Path.Combine(root, path));

		public Task<Stream> ReadFile(string path) => ioProvider.ReadFile(Path.Combine(root, path));

		public Task<Stream> WriteFile(string path) => ioProvider.WriteFile(Path.Combine(root, path));

		public Task<Stream> AppendFile(string path) => ioProvider.AppendFile(Path.Combine(root, path));
	}
}
