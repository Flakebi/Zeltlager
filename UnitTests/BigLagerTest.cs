using System.Threading.Tasks;

using NUnit.Framework;

using Zeltlager;
using Zeltlager.Client;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;

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
			// Unload the lager
			lager.Unload();
			// Load the bundles again
			Assert.AreEqual(true, await lager.LoadBundles());

			// Unload the whole manager
			IIoProvider ioProvider = manager.IoProvider;
			manager = null;
			lager = null;
			ownCollaborator = null;
			serialiser = null;
			context = null;
			// Load everything again
			manager = new LagerClientManager(ioProvider);
			await manager.Load();
			lager = (LagerClient)manager.Lagers[0];
			ownCollaborator = lager.OwnCollaborator;
			serialiser = lager.ClientSerialiser;
			context = new LagerClientSerialisationContext(manager, lager);
			context.PacketId = new PacketId(ownCollaborator);
			Assert.AreEqual(true, await lager.LoadBundles());
		}
	}
}
