using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.Client
{
	using DataPackets;
	using Serialisation;

	// This is the logic part of the LagerClient, the saving/network/etc. part
	// can be found in LagerClientCommunication.cs.
	/// <summary>
	/// Serialising a LagerClient with a LagerClientSerialisationContext will write
	/// the password of a lager,
	/// the collaborator private key,
	/// the id of this lager on the server,
	/// the order of the collaborators on the server (as a list),
	/// the lager status of the server.
	/// Reading with a LagerClientSerialisationContext will also create the collaborator list.
	/// </summary>
	public partial class LagerClient : LagerBase, ISerialisable<LagerSerialisationContext>, ISerialisable<LagerClientSerialisationContext>, ISearchable
	{
		public IReadOnlyList<Member> Members => members;
		public IReadOnlyList<Tent> Tents => tents;
		public IReadOnlyList<Member> Supervisors { get { return new List<Member>(Members.Where(x => x.Supervisor == true)); } }

		public LagerClientManager ClientManager { get; private set; }

		// Subspaces
		public Competition.CompetitionHandler CompetitionHandler { get; private set; }
		public Erwischt.Erwischt Erwischt { get; private set; }
		public Calendar.Calendar Calendar { get; private set; }

		List<Member> members;
		List<Tent> tents;

		// Searchable implementation
		public string SearchableText => Data.Name;
		public string SearchableDetail => password;

		public LagerClient(LagerClientManager manager, IIoProvider ioProvider, int id) :
			base(manager, ioProvider, id)
		{
			ClientManager = manager;
			Reset();
		}

		/// <summary>
		/// Resets all loaded data of this lager instance.
		/// This should be used before reloading the history.
		/// </summary>
		public void Reset()
		{
			// Reset collaborators
			foreach (var col in Collaborators.Values)
				col.Collaborators.Clear();
			
			// Reset content
			members = new List<Member>();
			tents = new List<Tent>();

			CompetitionHandler = new Competition.CompetitionHandler(this);
			Erwischt = new Erwischt.Erwischt(this);
			Calendar = new Calendar.Calendar(this);
		}

		/// <summary>
		/// Assemble the packet history from all collaborators.
		/// This orders the packets in chronological order and removes packet bundles.
		/// The packet that should be applied first, is the last packet in the list.
		/// </summary>
		/// <returns>The flat history of packets.</returns>
		async Task<List<DataPacket>> GetHistory()
		{
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Manager, this);
			return (await Task.WhenAll(collaborators.Values.Select(async col =>
			{
				context.PacketId = new PacketId(col);
				return (await Task.WhenAll(col.Bundles.Select(async b =>
				{
					try
					{
						return await b.GetPackets(context);
					}
					catch (Exception e)
					{
						// Log the exception
						await LagerManager.Log.Exception("Load bundle", e);
						return new List<DataPacket>();
					}
				}))).SelectMany(p => p);
			})))
				// Use OrderBy which is a stable sorting algorithm
				.SelectMany(p => p)
				.OrderBy(packet => packet).Reverse().ToList();
		}

		/// <summary>
		/// Apply the whole history.
		/// </summary>
		/// <returns>
		/// Returns true if the whole history was applied successfully,
		/// false if an error occured.
		/// </returns>
		public async Task<bool> ApplyHistory()
		{
			List<DataPacket> wholeHistory = await GetHistory();
			List<DataPacket> history = wholeHistory.ToList();
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Manager, this);
			context.Packets = history;
			bool success = true;
			while (history.Any())
			{
				var packet = history.Last();
				history.RemoveAt(history.Count - 1);
				try
				{
					context.PacketId = packet.Id;
					await packet.Deserialise(ClientSerialiser, context);
				}
				catch (Exception e)
				{
					// Log the exception
					await LagerManager.Log.Exception("Apply history", e);
					success = false;
				}
			}
			return success;
		}

		public void AddMember(Member member)
		{
			if (Members.Any(m => m.Id == member.Id))
				throw new LagerException("A member with this id exists already.");
			members.Add(member);
		}

		public void RemoveMember(Member member)
		{
			if (!members.Remove(member))
				throw new LagerException("A member with this id wasn't found for deletion.");
		}

		public void AddTent(Tent tent)
		{
			if (Tents.Any(t => t.Id == tent.Id))
				throw new LagerException("A tent with this id exists already.");
			tents.Add(tent);
		}

		public void RemoveTent(Tent tent)
		{
			if (!tents.Remove(tent))
				throw new LagerException("A tent with this id wasn't found for deletion.");
		}

		public Tent GetTentFromDisplay(string display)
		{
			Tent t = null;
			// find correct tent from display string
			foreach (Tent tent in Tents)
			{
				if (tent.Display == display)
				{
					t = tent;
					break;
				}
			}
			return t;
		}

		public Member GetMemberFromString(string memberstring)
		{
			return Members.First(x => x.ToString() == memberstring);
		}

		/// <summary>
		/// Create some test data if a new lager is created
		/// </summary>
		public async Task CreateTestData()
		{
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Manager, this);
			context.PacketId = new PacketId(OwnCollaborator);

			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Tent(null, 0, "Tiger", false, new List<Member>(), context.LagerClient)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Tent(null, 1, "Giraffen", false, new List<Member>(), context.LagerClient)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Tent(null, 2, "Seepferdchen", false, new List<Member>(), context.LagerClient)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Tent(null, 3, "Pinguine", false, new List<Member>(), context.LagerClient)));

			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Anna", Tents.Skip(new Random().Next(0, Tents.Count)).First(), true, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Bernd", Tents.Skip(new Random().Next(0, Tents.Count)).First(), true, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Claudius", Tents.Skip(new Random().Next(0, Tents.Count)).First(), false, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Don", Tents.Skip(new Random().Next(0, Tents.Count)).First(), false, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Emily", Tents.Skip(new Random().Next(0, Tents.Count)).First(), false, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Franz", Tents.Skip(new Random().Next(0, Tents.Count)).First(), false, this)));
		}
	}
}
