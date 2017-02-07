using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Zeltlager;
using Zeltlager.DataPackets;

namespace UnitTests
{
	[TestFixture]
	public class RevertPacketTests : LagerTest
	{
		[Test]
		public void RevertPacket()
		{
			Task.WaitAll(RevertPacketAsync());
		}

		public async Task RevertPacketAsync()
		{
			await Init();
			// Create a RevertPacket for the first member
			Member member = lager.Members.First();
			await lager.AddPacket(new RevertPacket(serialiser, context, member.Id));
			// Check that the member doesn't exist no more
			Assert.AreEqual(false, lager.Members.Any(m => m.Id == member.Id));
		}
	}
}
