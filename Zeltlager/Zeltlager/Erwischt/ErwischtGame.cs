using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace Zeltlager.Erwischt
{
	using Client;
	using DataPackets;
	using Newtonsoft.Json;
		using UAM;

	/// <summary>
	/// One instance of an Erwischt game.
	/// </summary>
	[Editable("Erwischtspiel")]
	public class ErwischtGame : Editable<ErwischtGame>, ISearchable
	{
		LagerClient lager;

		[JsonIgnore]
		public PacketId Id { get; set; }

		[Editable("Name")]
		public string Name { get; set; }

		public List<ErwischtParticipant> ErwischtParticipants { get; private set; }

		[JsonIgnore]
		public string SearchableText => Name;

		[JsonIgnore]
		public string SearchableDetail
		{
			get
			{
				StringBuilder res = new StringBuilder();
				res.Append(ErwischtParticipants.Where(em => em.IsAlive).Count());
				res.Append("/");
				res.Append(ErwischtParticipants.Count);
				res.Append(" verbleibende Teilnehmer");

				res.Append(" (");
				res.Append(Id.Packet.Timestamp.ToString("dd.MM.yy HH:mm:ss"));
				res.Append(")");
				return res.ToString();
			}
		}

		public bool IsVisible { get; set; } = true;

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
				ErwischtParticipants.Add(new ErwischtParticipant(m, 0, this));
			}
		}

		public void ShuffleParticipantTargets()
		{
			ErwischtParticipants.Shuffle();
		}

		public void AssignTargetsToParticipants()
		{
			for (int i = 0; i < ErwischtParticipants.Count; i++)
				ErwischtParticipants[i].SetIndex(i);
		}

		public LagerClient GetLager()
		{
			return lager;
		}
	}
}
