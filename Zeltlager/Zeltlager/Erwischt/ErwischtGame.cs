using System.Collections.Generic;
using Zeltlager.Client;
using Zeltlager.Competition;
using System.Linq;
using Zeltlager.DataPackets;

namespace Zeltlager.Erwischt
{
	using UAM;
	using Serialisation;
	using System;
	using System.Threading.Tasks;

	/// <summary>
	/// One instance of an Erwischt game.
	/// </summary>
	[Editable("Erwischtspiel")]
	public class ErwischtGame : Editable<ErwischtGame>, ISearchable, IDeletable
	{
		LagerClient lager;

		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Serialisation]
		[Editable("Name")]
		public string Name { get; set; }

		[Serialisation]
		public List<ErwischtParticipant> ErwischtParticipants { get; set; }

		public string SearchableText => Name;

		public string SearchableDetail => ErwischtParticipants.Where(em => em.IsAlive).Count() + " verbleibende Teilnehmer";

		public bool IsVisible { get; set; }

		// For deserialisation
		protected static Task<ErwischtGame> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.ErwischtHandler.Games.First(g => g.Id == id));
		}

		public ErwischtGame(string name, LagerClient lager)
		{
			this.lager = lager;
			Name = name;
			ErwischtParticipants = new List<ErwischtParticipant>();
			InitNewGame();
		}

		ErwischtGame(string name, List<ErwischtParticipant> participants, LagerClient lager) : this(name, lager)
		{
			ErwischtParticipants = participants;
			AssignTargetsToParticipants();
		}

		// For deserialisation
		public ErwischtGame(LagerClientSerialisationContext context) : this(null, context.LagerClient) { }

		public void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			lager = context.LagerClient;
			AssignTargetsToParticipants();
			context.LagerClient.ErwischtHandler.Games.Add(this);
		}

		public override ErwischtGame Clone()
		{
			return new ErwischtGame(Name, ErwischtParticipants, lager);
		}

		public void InitNewGame()
		{
			FillParticipantsWithLagerMembers();
			ShuffleParticipantTargets();
			AssignTargetsToParticipants();
		}

		public void FillParticipantsWithLagerMembers()
		{
			ErwischtParticipants = new List<ErwischtParticipant>();
			foreach (Member m in lager.VisibleMembers)
			{
				ErwischtParticipants.Add(new ErwischtParticipant(m, null, this));
			}
		}

		public void ShuffleParticipantTargets()
		{
			ErwischtParticipants.Shuffle();
		}

		public void AssignTargetsToParticipants()
		{
			for (int i = 0; i < ErwischtParticipants.Count; i++)
			{
				ErwischtParticipants[i].SetInitialTarget(ErwischtParticipants[(i + 1) % ErwischtParticipants.Count]);
			}
		}

		public LagerClient GetLager()
		{
			return lager;
		}
	}
}