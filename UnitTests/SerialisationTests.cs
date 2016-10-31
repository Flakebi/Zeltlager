using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using NUnit.Framework;

using Zeltlager;
using Zeltlager.Client;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;

namespace UnitTests
{
	[TestFixture]
	public class SerialisationTests
	{
		bool inited;

		LagerClientManager manager;
		LagerClient lager;
		Collaborator ownCollaborator;
		Serialiser<LagerClientSerialisationContext> serialiser;
		LagerClientSerialisationContext context;

		async Task Init()
		{
			if (inited)
				return;
			inited = true;

			LagerManager.IsClient = true;

			manager = new LagerClientManager(new DiscardIoProvider());
			await manager.CreateLager("Testlager", "secure passwörd", null);
			lager = (LagerClient)manager.Lagers[0];
			ownCollaborator = lager.OwnCollaborator;
			serialiser = new Serialiser<LagerClientSerialisationContext>();
			context = new LagerClientSerialisationContext(manager, lager);
			context.PacketId = new PacketId(ownCollaborator);
		}
		
		[Test]
		public void SerialiseTent()
		{
			Task.WaitAll(SerialiseTentAsync());
		}

		public async Task SerialiseTentAsync()
		{
			await Init();

			Tent tent = new Tent(null, 0, "Tent", false, new List<Member>());
			//Member member = new Member(null, "Member", tent, true);
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				await serialiser.Write(output, context, tent);
			}

			mem = new MemoryStream(mem.ToArray());
			using (BinaryReader input = new BinaryReader(mem))
			{
				Tent t = new Tent();
				await serialiser.Read(input, context, t);
			}
		}
	}
}
