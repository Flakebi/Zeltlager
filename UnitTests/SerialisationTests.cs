using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
		Semaphore semaphore = new Semaphore(1, 1);

		LagerClientManager manager;
		LagerClient lager;
		Collaborator ownCollaborator;
		Serialiser<LagerClientSerialisationContext> serialiser;
		LagerClientSerialisationContext context;
		Tent tent;
		Member member;

		async Task Init()
		{
			if (inited)
				return;

			// Lock
			try
			{
				semaphore.WaitOne();
				if (inited)
					return;

				LagerManager.IsClient = true;

				manager = new LagerClientManager(new DiscardIoProvider());
				await manager.CreateLager("Testlager", "secure passwörd", null);
				lager = (LagerClient)manager.Lagers[0];
				ownCollaborator = lager.OwnCollaborator;
				serialiser = new Serialiser<LagerClientSerialisationContext>();
				context = new LagerClientSerialisationContext(manager, lager);
				context.PacketId = new PacketId(ownCollaborator);

				tent = new Tent(null, 0, "Tent", false, new List<Member>(), lager);
				await lager.AddPacket(await AddPacket.Create(serialiser, context, tent));
				// Get the newly created objects
				tent = lager.Tents.First();

				member = new Member(null, "Member", tent, true, lager);
				await lager.AddPacket(await AddPacket.Create(serialiser, context, member));
				member = lager.Members.First();

				inited = true;
			} finally
			{
				semaphore.Release();
			}
		}

		[Test]
		public void SerialiseTent()
		{
			Task.WaitAll(SerialiseTentAsync());
		}

		public async Task SerialiseTentAsync()
		{
			await Init();
			
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
				Assert.AreEqual(tent, t);
			}
		}

		[Test]
		public void SerialiseMember()
		{
			Task.WaitAll(SerialiseMemberAsync());
		}

		public async Task SerialiseMemberAsync()
		{
			await Init();

			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				await serialiser.Write(output, context, member);
			}

			mem = new MemoryStream(mem.ToArray());
			using (BinaryReader input = new BinaryReader(mem))
			{
				Member m = new Member(lager);
				await serialiser.Read(input, context, m);
				Assert.AreEqual(member, m);
			}
		}
	}
}
