using System;
using System.Threading;
using System.Threading.Tasks;

using Zeltlager;
using Zeltlager.Client;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;

namespace UnitTests
{
	public class LagerTest
	{
		bool inited;
		readonly Semaphore semaphore = new Semaphore(1, 1);

		protected LagerClientManager manager;
		protected LagerClient lager;
		protected Collaborator ownCollaborator;
		protected Serialiser<LagerClientSerialisationContext> serialiser;
		protected LagerClientSerialisationContext context;

		protected async Task Init()
		{
			if (inited)
				return;

			// Lock using semaphores because async is multi-threaded
			try
			{
				semaphore.WaitOne();

				if (inited)
					return;

				LagerManager.IsClient = true;

				manager = new LagerClientManager(new MemoryIoProvider());
				LagerManager.Log.OnMessage += Console.WriteLine;
				await manager.CreateLager("Testlager", "secure passwörd", null);
				lager = (LagerClient)manager.Lagers[0];
				ownCollaborator = lager.OwnCollaborator;
				serialiser = lager.ClientSerialiser;
				context = new LagerClientSerialisationContext(lager);
				context.PacketId = new PacketId(ownCollaborator);
				await lager.CreateTestData();

				inited = true;
			}
			finally
			{
				semaphore.Release();
			}
		}
	}
}
