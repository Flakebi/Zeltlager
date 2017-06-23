using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.Client
{
	using Competition;
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

		// For the GUI, deleted items not included
		public IReadOnlyList<Member> VisibleMembers => members.Where(m => m.IsVisible).ToList();
		public IReadOnlyList<Tent> VisibleTents => tents.Where(t => t.IsVisible).ToList();
		public IReadOnlyList<Member> VisibleSupervisors { get { return new List<Member>(Members.Where(x => x.Supervisor == true && x.IsVisible)); } }

		public LagerClientManager ClientManager { get; private set; }

		// Subspaces
		public CompetitionHandler CompetitionHandler { get; private set; }
		public Erwischt.ErwischtHandler ErwischtHandler { get; private set; }
		public Calendar.Calendar Calendar { get; private set; }

		List<Member> members;
		List<Tent> tents;

		// Searchable implementation
		public string SearchableText => Data.Name;
		public string SearchableDetail => password;

		public string Password => password;

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
			// Reset content
			members = new List<Member>();
			tents = new List<Tent>();

			CompetitionHandler = new CompetitionHandler(this);
			ErwischtHandler = new Erwischt.ErwischtHandler(this);
			Calendar = new Calendar.Calendar(this);
		}

		/// <summary>
		/// Assemble the packet history from all collaborators.
		/// This orders the packets in chronological order and removes packet bundles.
		/// The packet that should be applied first, is the last packet in the list.
		/// </summary>
		/// <returns>The flat history of packets.</returns>
		public async Task<List<DataPacket>> GetHistory()
		{
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(this);
			List<DataPacket> packets = new List<DataPacket>();
			foreach (var col in collaborators.Values)
			{
				context.PacketId = new PacketId(col);
				foreach (var b in col.Bundles)
				{
					try
					{
						packets.AddRange(await b.GetPackets(context));
					}
					catch (Exception e)
					{
						// Log the exception
						await LagerManager.Log.Exception("Load bundle " + b.Id + " of " + col, e);
					}
				}
			}
			// Use OrderBy which is a stable sorting algorithm
			return packets.OrderBy(packet => packet).Reverse().ToList();
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
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(this);
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

		public Tent GetRandomTent(Random rand) => Tents[rand.Next(0, Tents.Count)];
		public Member GetRandomMember(Random rand) => Members[rand.Next(0, Members.Count)];

		/// <summary>
		/// Create some test data if a new lager is created
		/// </summary>
		public async Task CreateTestData()
		{
			Random rand = new Random();

			LagerClientSerialisationContext context = new LagerClientSerialisationContext(this);
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
				new Member(null, "Anna", GetRandomTent(rand), true, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Bernd", GetRandomTent(rand), true, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Claudius", GetRandomTent(rand), false, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Don", GetRandomTent(rand), false, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Emily", GetRandomTent(rand), false, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Franz", GetRandomTent(rand), false, this)));

			// Add a competition
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Competition(null, "Ostfriesenwettkampf", this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Station(null, "Kartenweitwurf", CompetitionHandler.Competitions.First(), GetRandomMember(rand))));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Station(null, "Teebeutellasso", CompetitionHandler.Competitions.First(), GetRandomMember(rand))));

			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new TentParticipant(null, Tents[0], CompetitionHandler.Competitions.First())));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new TentParticipant(null, Tents[1], CompetitionHandler.Competitions.First())));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new MemberParticipant(null, Members[0], CompetitionHandler.Competitions.First())));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new MemberParticipant(null, Members[1], CompetitionHandler.Competitions.First())));

			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new CompetitionResult(null, CompetitionHandler.Competitions.First().Stations[0],
				CompetitionHandler.Competitions.First().Participants[0], 1)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new CompetitionResult(null, CompetitionHandler.Competitions.First().Stations[1],
				CompetitionHandler.Competitions.First().Participants[2], 2)));
		}
	}
}
