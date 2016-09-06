using System.IO;
using System.Threading.Tasks;

namespace Zeltlager
{
	public interface IIoProvider
	{
		Task CreateFolder(string path);
		Task<bool> ExistsFile(string path);
		Task<bool> ExistsFolder(string path);
		Task<Stream> WriteFile(string path);
		Task<Stream> AppendFile(string path);
		Task<Stream> ReadFile(string path);
	}
}
