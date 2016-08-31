using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager
{
	public interface IIoProvider
	{
		Task CreateFolder(string path);
		Task<BinaryWriter> WriteFile(string file);
		Task<BinaryReader> ReadFile(string file);
	}
}
