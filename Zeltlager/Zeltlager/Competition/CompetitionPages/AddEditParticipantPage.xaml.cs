using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;
using System.Linq;

namespace Zeltlager.Competition
{
	public partial class AddEditParticipantPage : ContentPage
	{
		Participant participant;
		Participant oldParticipant;
		Grid grid;

		const string AUTOMATIC = "automatisch Hinzufügen";
		const string TENT = "ganzes Zelt";
		const string MEMBER = "einzelner Teilnehmer";
		const string GROUP = "gemischte Gruppe";

		public AddEditParticipantPage(Participant participant, bool isAddPage)
		{
			InitializeComponent();
			this.participant = participant.Clone();
			if (isAddPage)
				oldParticipant = null;
			else
				oldParticipant = participant;

			grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			Label label = new Label
			{
				Text = "Typ: ",
				TextColor = (Color)Application.Current.Resources["textColorSecondary"],
			};
			grid.Children.Add(label, 0, 0);
			Picker typePicker = new Picker();
			typePicker.Items.Add(TENT);
			typePicker.Items.Add(MEMBER);
			typePicker.Items.Add(GROUP);
			typePicker.Items.Add(AUTOMATIC);
			typePicker.SelectedIndexChanged += (sender, args) =>
			{
				UpdateUI(typePicker.Items[typePicker.SelectedIndex]);
			};
			grid.Children.Add(typePicker, 1, 0);
			Content = grid;
			ToolbarItems.Add(new ToolbarItem(null, Icons.CANCEL, OnCancelClicked, ToolbarItemOrder.Primary, 0));
			ToolbarItems.Add(new ToolbarItem(null, Icons.SAVE, OnSaveClicked, ToolbarItemOrder.Primary, 1));
			Style = (Style)Application.Current.Resources["BaseStyle"];
			Padding = new Thickness(10);
			Title = "Wettbewerbsteilnehmer";
			NavigationPage.SetBackButtonTitle(this, "");
		}

		void UpdateUI(string selection)
		{
			Label label = new Label
			{
				TextColor = (Color)Application.Current.Resources["textColorSecondary"],
			};
			View manip = new Button();
			switch (selection)
			{
				case AUTOMATIC:
					Button allTents = new Button
					{
						Text = "Alle Zelte hinzufügen",
						Style = (Style)Application.Current.Resources["DarkButtonStyle"],
					};
					allTents.Clicked += AddAllTents;
					Button allMembers = new Button
					{
						Text = "Alle Teilnehmer hinzufügen",
						Style = (Style)Application.Current.Resources["DarkButtonStyle"],
					};
					allMembers.Clicked += AddAllMembers;
					grid.Children.Add(allTents, 0, 2, 1, 2);
					grid.Children.Add(allMembers, 0, 2, 2, 3);
					// we don't want the rest of the gui to be added
					return;
				case TENT:
					label.Text = "Zelt wählen: ";
					Picker picker = new Picker();
					IReadOnlyList<Tent> list = participant.GetLagerClient().VisibleTents;
					foreach (Tent tent in list)
					{
						picker.Items.Add(tent.ToString());
					}
					picker.SelectedIndexChanged += (sender, args) =>
					{
						Tent t = participant.GetLagerClient().GetTentFromDisplay(picker.Items[picker.SelectedIndex]);
						participant = new TentParticipant(null, t, participant.GetCompetition());
					};
					picker.SelectedIndex = 0;
					manip = picker;
					break;
				case MEMBER:
					label.Text = "Teilnehmer wählen: ";
					Picker memberpicker = new Picker();
					IReadOnlyList<Member> memberlist = participant.GetLagerClient().VisibleMembers;
					foreach (Member mem in memberlist)
					{
						memberpicker.Items.Add(mem.ToString());
					}
					memberpicker.SelectedIndexChanged += (sender, args) =>
					{
						Member m = participant.GetLagerClient().GetMemberFromString(memberpicker.Items[memberpicker.SelectedIndex]);
						participant = new MemberParticipant(null, m, participant.GetCompetition());
					};
					memberpicker.SelectedIndex = 0;
					manip = memberpicker;
					break;
				case GROUP:
					label.Text = "Gruppenname: ";
					manip = new Entry();
					participant = new GroupParticipant(null, "", participant.GetCompetition());
					manip.BindingContext = participant;
					manip.SetBinding(Entry.TextProperty, new Binding("Name", BindingMode.TwoWay));
					break;
			}
			grid.Children.Add(label, 0, 1);
			grid.Children.Add(manip, 1, 1);
		}

		void OnCancelClicked()
		{
			Navigation.PopModalAsync(true);
		}

		async void OnSaveClicked()
		{
			// check if this participant already exists
			if (participant.GetCompetition().Participants.Contains(participant))
			{
				await DisplayAlert("Achtung!", "Es gibt diesen Teilnehmer bereits.", "Ok :D");
				return;
			}
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(participant.GetLagerClient().Manager, participant.GetLagerClient());
			context.PacketId = new PacketId(participant.GetLagerClient().OwnCollaborator);
			await participant.OnSaveEditing(participant.GetLagerClient(), oldParticipant);
			await Navigation.PopModalAsync(true);
		}

		async void AddAllTents(object sender, EventArgs e)
		{
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(participant.GetLagerClient().Manager, participant.GetLagerClient());
			context.PacketId = new PacketId(participant.GetLagerClient().OwnCollaborator);
			foreach (Tent t in participant.GetLagerClient().VisibleTents)
			{
				TentParticipant par = new TentParticipant(null, t, participant.GetCompetition());
				await par.OnSaveEditing(participant.GetLagerClient(), null);
			}
			await Navigation.PopModalAsync(true);
		}

		async void AddAllMembers(object sender, EventArgs e)
		{
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(participant.GetLagerClient().Manager, participant.GetLagerClient());
			context.PacketId = new PacketId(participant.GetLagerClient().OwnCollaborator);
			foreach (Member m in participant.GetLagerClient().VisibleMembers)
			{
				MemberParticipant par = new MemberParticipant(null, m, participant.GetCompetition());
				await par.OnSaveEditing(participant.GetLagerClient(), null);
			}
			await Navigation.PopModalAsync(true);
		}
	}
}
