using System;
using System.Threading.Tasks;
using Zeltlager.Client;
using Zeltlager.UAM;
namespace Zeltlager.Competition
{
	/// <summary>
	/// represents a participant in a comptetion, could be a tent, a mixed group or a single person
	/// </summary>
	[Editable("Teilnehmer")]
	public class Participant : ISearchable, IEditable<Participant>
	{
		Competition competition;

		[Editable("Name")]
		string name;

		public Participant(string name, Competition competition)
		{
			this.name = name;
			this.competition = competition;
		}

		#region Interface implementation

		public string SearchableText { get { return name; } }

		public string SearchableDetail { get { return ""; } }

		public Task OnSaveEditing(Participant oldObject, LagerClient lager)
		{
			// TODO: Packets
			return null;
		}

		public Participant Clone()
		{
			return new Participant(name, competition);
		}

		#endregion
	}
}
