using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

namespace Zeltlager.Competition
{
	using Client;
	using DataPackets;
	using Serialisation;

	public partial class AddEditParticipantPage : ContentPage
	{
		Participant participant;
		Participant oldParticipant;

		Dictionary<string, RowDefinition> rows = new Dictionary<string, RowDefinition>();
		Dictionary<string, View[]> rowsContent = new Dictionary<string, View[]>();

		public AddEditParticipantPage(Participant participant, bool isAddPage)
		{
			InitializeComponent();
			this.participant = participant.Clone();
			if (isAddPage)
				oldParticipant = null;
			else
				oldParticipant = participant;

			// Tents
			IEnumerable<Tent> tents = participant.GetLagerClient().VisibleTents.Except(
				participant.GetCompetition().Participants.Where(p => p is TentParticipant)
				.Cast<TentParticipant>()
				.Select(p => p.Tent));
			foreach (Tent tent in tents)
				tentPicker.Items.Add(tent.ToString());
			tentPicker.SelectedIndex = 0;
			rows[TENT] = tentRow;
			rowsContent[TENT] = new View[] { tentLabel, tentPicker };

			// Members
			IEnumerable<Member> members = participant.GetLagerClient().VisibleMembers.Except(
				participant.GetCompetition().Participants.Where(p => p is MemberParticipant)
				.Cast<MemberParticipant>()
				.Select(p => p.Member));
			foreach (Member member in members)
				memberPicker.Items.Add(member.ToString());
			memberPicker.SelectedIndex = 0;
			rows[MEMBER] = memberRow;
			rowsContent[MEMBER] = new View[] { memberLabel, memberPicker };

			rows[GROUP] = groupRow;
			rowsContent[GROUP] = new View[] { groupLabel, groupEntry };
			rows[AUTOMATIC] = automaticRow;
			rowsContent[AUTOMATIC] = new View[] { automaticView };

			if (isAddPage)
				typePicker.SelectedIndex = 0;
			else
			{
				if (participant is TentParticipant)
				{
					TentParticipant tp = (TentParticipant)participant;
					tentPicker.Items.Add(tp.Tent.ToString());
					tentPicker.SelectedIndex = tentPicker.Items.Count - 1;
					typePicker.SelectedIndex = 0;
				}
				else if (participant is MemberParticipant)
				{
					MemberParticipant mp = (MemberParticipant)participant;
					memberPicker.Items.Add(mp.Member.ToString());
					memberPicker.SelectedIndex = memberPicker.Items.Count - 1;
					typePicker.SelectedIndex = 1;
				}
				else if (participant is GroupParticipant)
				{
					GroupParticipant gp = (GroupParticipant)participant;
					groupEntry.Text = gp.Name;
					typePicker.SelectedIndex = 2;
				}
				typeLabel.IsVisible = false;
				typePicker.IsVisible = false;
			}

			NavigationPage.SetBackButtonTitle(this, "");
		}

		void OnTypeChanged(object o, EventArgs e)
		{
			string selection = typePicker.Items[typePicker.SelectedIndex];
			// Make the right rows visible
			foreach (var v in rowsContent.Values.SelectMany(v => v))
				v.IsVisible = false;
			foreach (var row in rows.Values)
				row.Height = 0;

			foreach (var v in rowsContent[selection])
				v.IsVisible = true;
			rows[selection].Height = GridLength.Auto;
		}

		void OnCancelClicked(object o, EventArgs e)
		{
			Navigation.PopAsync(true);
		}

		async void OnSaveClicked(object o, EventArgs e)
		{
			LagerClient lager = participant.GetLagerClient();

			string selection = typePicker.Items[typePicker.SelectedIndex];
			// Create the new participant
			if (selection == TENT)
			{
				if (tentPicker.SelectedIndex == -1)
				{
					await DisplayAlert("Achtung!", "Bitte wähle ein Zelt aus.", "Ok");
					return;
				}
				Tent t = lager.GetTentFromDisplay(tentPicker.Items[tentPicker.SelectedIndex]);
				if (oldParticipant == null)
					participant = new TentParticipant(null, t, participant.GetCompetition());
				else
					((TentParticipant)participant).Tent = t;
			}
			else if (selection == MEMBER)
			{
				if (memberPicker.SelectedIndex == -1)
				{
					await DisplayAlert("Achtung!", "Bitte wähle einen Teilnehmer aus.", "Ok");
					return;
				}
				Member m = lager.GetMemberFromString(memberPicker.Items[memberPicker.SelectedIndex]);
				if (oldParticipant == null)
					participant = new MemberParticipant(null, m, participant.GetCompetition());
				else
					((MemberParticipant)participant).Member = m;
			}
			else if (selection == GROUP)
			{
				if (string.IsNullOrEmpty(groupEntry.Text))
				{
					await DisplayAlert("Achtung!", "Bitte gib einen Gruppennamen ein.", "Ok");
					return;
				}
				if (oldParticipant == null)
					participant = new GroupParticipant(null, groupEntry.Text, participant.GetCompetition());
				else
					((GroupParticipant)participant).Name = groupEntry.Text;
			}

			// Check if this participant already exists
			if (participant.GetCompetition().Participants.Contains(participant))
			{
				await DisplayAlert("Achtung!", "Es gibt diesen Teilnehmer bereits.", "Ok");
				return;
			}
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(
				lager.Manager, lager);
			context.PacketId = new PacketId(lager.OwnCollaborator);
			await participant.OnSaveEditing(lager, oldParticipant);
			await Navigation.PopAsync(true);
		}

		async void AddAllTents(object sender, EventArgs e)
		{
			LagerClient lager = participant.GetLagerClient();
			Competition competition = participant.GetCompetition();
			IEnumerable<Tent> participatingTents = competition.Participants
				.Where(p => p is TentParticipant)
				.Cast<TentParticipant>()
				.Select(p => p.Tent);

			LagerClientSerialisationContext context = new LagerClientSerialisationContext(
				lager.Manager, lager);
			context.PacketId = new PacketId(lager.OwnCollaborator);
			foreach (Tent t in lager.VisibleTents.Except(participatingTents))
			{
				TentParticipant par = new TentParticipant(null, t, competition);
				await par.OnSaveEditing(lager, null);
			}
			await Navigation.PopAsync(true);
		}

		async void AddAllMembers(object sender, EventArgs e)
		{
			LagerClient lager = participant.GetLagerClient();
			Competition competition = participant.GetCompetition();
			IEnumerable<Member> participatingMembers = competition.Participants
				.Where(p => p is MemberParticipant)
				.Cast<MemberParticipant>()
				.Select(p => p.Member);

			LagerClientSerialisationContext context = new LagerClientSerialisationContext(
				lager.Manager, lager);
			context.PacketId = new PacketId(lager.OwnCollaborator);
			foreach (Member m in lager.VisibleMembers.Except(participatingMembers))
			{
				MemberParticipant par = new MemberParticipant(null, m, competition);
				await par.OnSaveEditing(lager, null);
			}
			await Navigation.PopAsync(true);
		}
	}
}
