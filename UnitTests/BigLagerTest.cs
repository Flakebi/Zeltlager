using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Zeltlager;
using Zeltlager.Client;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;
using Zeltlager.Calendar;

namespace UnitTests
{
	public class BigLagerTest : LagerTest
	{
		[Test]
		public void BigTest()
		{
			Task.WaitAll(BigTestAsync());
		}

		async Task Reload()
		{
			// Save the unencrypted data to compare the afterwards
			List<byte[]> unencryptedBundles = new List<byte[]>();
			foreach (var bundle in ownCollaborator.Bundles)
			{
				await bundle.Serialise(context);
				unencryptedBundles.Add(bundle.Pack());
			}

			// Unload the whole manager
			IIoProvider ioProvider = manager.IoProvider;
			manager = null;
			lager = null;
			ownCollaborator = null;
			serialiser = null;
			context = null;
			// Load everything again
			manager = new LagerClientManager(ioProvider);
			LagerManager.Log.OnMessage += Console.WriteLine;
			await manager.Load();
			lager = (LagerClient)manager.Lagers[0];
			ownCollaborator = lager.OwnCollaborator;
			serialiser = lager.ClientSerialiser;
			context = new LagerClientSerialisationContext(lager);
			context.PacketId = new PacketId(ownCollaborator);
			Assert.True(await lager.LoadBundles());

			// Check if the unencrypted data are still the same
			for (int i = 0; i < unencryptedBundles.Count; i++)
			{
				byte[] unencrypted = unencryptedBundles[i];

				var bundle = ownCollaborator.Bundles[i];
				var verificationResult = await bundle.VerifyAndGetEncryptedData(context);
				byte[] unencryptedData = await LagerManager.CryptoProvider.DecryptSymetric(
					context.LagerClient.Data.SymmetricKey, verificationResult.Item1, verificationResult.Item2);

				Assert.True(unencrypted.SequenceEqual(unencryptedData.Take(unencrypted.Length)));
				Assert.True(unencrypted.SequenceEqual(unencryptedData));
			}

			Assert.True(await lager.ApplyHistory());
		}

		public async Task BigTestAsync()
		{
			await Init();
			// Unload the lager
			lager.Unload();
			// Load the bundles again
			Assert.True(await lager.LoadBundles());
			Assert.True(await lager.ApplyHistory());

			await Reload();

			// Add more packets
			// AddPacket
			foreach (var obj in new object[]
			{
				new Member(null, "TestMember", lager.Tents.First(), true, lager),
				new Tent(null, 11, "TestTent", true, new List<Member>(), lager),
				new CalendarEvent(null, DateTime.Now, "TestEvent", "", lager),
			})
				await lager.AddPacket(await AddPacket.Create(serialiser, context, obj));

			// EditPacket
			{
				var member = lager.Members.First().Clone();
				var tent = lager.Tents.First().Clone();
				var cev = lager.Calendar.Days.First().Events.First().GetEditableCalendarEvent().Clone();
				member.Name = "TestEditMember";
				tent.Name = "TestEditTent";
				tent.Number = 12;
				cev.Title = "EditCalendar";
				cev.Detail = "EditDetail";
				foreach (var obj in new object[] { member, tent, cev })
					await lager.AddPacket(await EditPacket.Create(serialiser, context, obj));
			}

			// Add more packets to get at least 3 bundles
			{
				int count = 0;
				Member member = new Member(null, null, lager.Tents.First(), false, lager);
				while (ownCollaborator.Bundles.Count < 3)
				{
					Member m = member.Clone();
					m.Name = "BulkMember" + count;
					await lager.AddPacket(await AddPacket.Create(serialiser, context, m));
				}
			}

			// Revert the last packet and the RevertPacket
			await lager.AddPacket(new RevertPacket(serialiser, context, (await ownCollaborator.Bundles.Last().GetPackets(context)).Last().Id));
			await lager.AddPacket(new RevertPacket(serialiser, context, (await ownCollaborator.Bundles.Last().GetPackets(context)).Last().Id));

			await Reload();

			// Add more packets to get at least 4 bundles
			{
				int count = 0;
				Tent tent = new Tent(null, count, null, false, new List<Member>(), lager);
				while (ownCollaborator.Bundles.Count < 4)
				{
					Tent t = tent.Clone();
					t.Name = "BulkTent" + count;
					t.Number = count;
					t.Girls = (count % 2) == 0;
					await lager.AddPacket(await AddPacket.Create(serialiser, context, t));
				}
			}

			await Reload();
		}
	}
}
