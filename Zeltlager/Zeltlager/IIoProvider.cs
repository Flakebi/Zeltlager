using System.IO;
using System.Threading.Tasks;

namespace Zeltlager
{
	public interface IIoProvider
	{
		Task CreateFolder(string path);
		Task<bool> ExistsFile(string path);
		Task<bool> ExistsFolder(string path);
		Task<BinaryWriter> WriteFile(string path);
		Task<BinaryReader> ReadFile(string path);
	}
}
