using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Zeltlager;
using Zeltlager.DataPackets;

namespace UnitTests
{
	public class BigLagerTest : LagerTest
	{
		[Test]
		public void BigTest()
		{
			Task.WaitAll(BigTestAsync());
		}

		public async Task BigTestAsync()
		{
			await Init();
			// Save the lager
			// Load the lager again
		}
	}
}
