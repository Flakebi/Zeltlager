using System.IO;
using System.Threading.Tasks;

namespace Zeltlager
{
	/// <summary>
	/// An IoProvider that executes everything relative to a root path.
	/// </summary>
	public class RoutedIoProvider : IIoProvider
	{
		IIoProvider ioProvider;
		string root;

		public RoutedIoProvider(IIoProvider ioProvider, string root)
		{
			this.ioProvider = ioProvider;
			this.root = root;
		}

		public Task CreateFolder(string path)
		{
			return ioProvider.CreateFolder(Path.Combine(root, path));
		}

		public Task<bool> ExistsFile(string path)
		{
			return ioProvider.ExistsFile(Path.Combine(root, path));
		}

		public Task<bool> ExistsFolder(string path)
		{
			return ioProvider.ExistsFolder(Path.Combine(root, path));
		}

		public Task<BinaryReader> ReadFile(string path)
		{
			return ioProvider.ReadFile(Path.Combine(root, path));
		}

		public Task<BinaryWriter> WriteFile(string path)
		{
			return ioProvider.WriteFile(Path.Combine(root, path));
		}
	}
}
