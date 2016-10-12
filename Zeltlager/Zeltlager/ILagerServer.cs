using System.Collections.Generic;

namespace Zeltlager
{
	/// <summary>
	/// This represents a server.
	/// </summary>
	public interface ILagerServer
	{
		IList<LagerBase> GetLagers();
	}
}
