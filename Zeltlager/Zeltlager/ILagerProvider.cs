using System.Collections.Generic;

namespace Zeltlager
{
	/// <summary>
	/// This represents the server.
	/// </summary>
	public interface ILagerProvider
	{
		IList<Lager> GetLagers();
	}
}
