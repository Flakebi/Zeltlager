using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Zeltlager;
using Zeltlager.DataPackets;

namespace UnitTests
{
	[TestFixture]
	public class PacketTests : LagerTest
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
            Member member = lager.Members[5];
			await lager.AddPacket(new RevertPacket(serialiser, context, member.Id));
			// Check that the member doesn't exist no more
			Assert.AreEqual(false, lager.Members.Any(m => m.Id == member.Id));
		}

		[Test]
		public void FullBundle()
		{
			Task.WaitAll(FullBundleAsync());
		}

		public async Task FullBundleAsync()
		{
			await Init();
			// Create so many packets that more than one bundle has to be created
			while (ownCollaborator.Bundles.Count <= 1)
				await lager.CreateTestData();
			// Load the history again
			lager.Reset();
			Assert.AreEqual(true, await lager.ApplyHistory());
		}
	}
}
