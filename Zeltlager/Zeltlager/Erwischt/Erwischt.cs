using System.Collections.Generic;
using Zeltlager.Client;
using Zeltlager.Competition;
using System.Linq;

namespace Zeltlager.Erwischt
{
	using UAM;
	using Serialisation;
	using System;

	/// <summary>
	/// One instance of an Erwischt game.
	/// </summary>
	[Editable("Erwischtspiel")]
	public class Erwischt : Editable<Erwischt>, ISearchable, IDeletable
	{
		LagerClient lager;

		[Serialisation]
		[Editable("Name")]
		public string Name { get; set; }

		[Serialisation(Type = SerialisationType.Full)]
		public List<ErwischtMember> Participants { get; set; }

		public List<ErwischtMember> VisibleParticipants => Participants.Where(p => p.IsVisible).ToList();

		public string SearchableText => Name;

		public string SearchableDetail => Participants.Where(em => em.IsAlive).Count() + " verbleibende Teilnehmer";

		public bool IsVisible { get; set; }

		public Erwischt(string name, LagerClient lager)
		{
			Name = name;
			Participants = new List<ErwischtMember>();
		}

		Erwischt(string name, List<ErwischtMember> participants, LagerClient lager) : this(name, lager)
		{
			Participants = participants;
		}

		public override Erwischt Clone()
		{
			return new Erwischt(Name, Participants, lager);
		}

		public void InitNewGame()
		{
			FillParticipantsWithLagerMembers();
			ShuffleParticipantTargets();
		}

		public void FillParticipantsWithLagerMembers()
		{
			Participants = new List<ErwischtMember>();
			foreach (Member m in lager.VisibleMembers)
			{
				Participants.Add(new ErwischtMember(m, null, this));
			}
		}

		public void ShuffleParticipantTargets()
		{
			Participants.Shuffle();
			for (int i = 0; i < Participants.Count; i++)
			{
				Participants[i].SetInitialTarget(Participants[(i + 1) % Participants.Count]);
			}
		}
	}
}
