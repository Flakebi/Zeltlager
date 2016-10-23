using System;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager
{
	public enum FileType
	{
		File,
		Folder,
	}

	public interface IIoProvider
	{
		/// <summary>
		/// List all contained files and folders.
		/// </summary>
		/// <param name="path">
		/// The path that should be searched for files and folders.
		/// </param>
		/// <returns>All present files and folders.</returns>
		Task<Tuple<string, FileType>[]> ListContents(string path);

		Task CreateFolder(string path);
		Task<bool> ExistsFile(string path);
		Task<bool> ExistsFolder(string path);
		Task<Stream> WriteFile(string path);
		Task<Stream> AppendFile(string path);
		Task<Stream> ReadFile(string path);
	}
}
