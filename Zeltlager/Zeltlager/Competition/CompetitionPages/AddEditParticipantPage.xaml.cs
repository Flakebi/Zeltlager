using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;

namespace Zeltlager.Competition
{
	public partial class AddEditParticipantPage : ContentPage
	{
		Participant participant;
		Participant oldParticipant;
		Grid grid;

		const string TENT = "ganzes Zelt";
		const string MEMBER = "einzelner Teilnehmer";
		const string GROUP = "gemischte Gruppe";

		public AddEditParticipantPage(Participant participant, bool isAddPage)
		{
			InitializeComponent();
			this.participant = participant;
			if (isAddPage)
				oldParticipant = null;
			else
				oldParticipant = participant;

			grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

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
		}

		void UpdateUI(string selection)
		{
			Label label = new Label
			{
				TextColor = (Color)Application.Current.Resources["textColorSecondary"],
			};
			View manip = new Button();
			switch(selection)
			{
				case TENT:
					label.Text = "Zelt wählen: ";
					Picker picker = new Picker();
					IReadOnlyList<Tent> list = participant.GetLagerClient().Tents;
					foreach (Tent tent in list)
					{
						picker.Items.Add(tent.ToString());
					}
					picker.SelectedIndexChanged += (sender, args) =>
					{
						Tent t = participant.GetLagerClient().GetTentFromDisplay(picker.Items[picker.SelectedIndex]);
						participant.Name = t.Display;
					};
					picker.SelectedIndex = 0;
					manip = picker;
					break;
				case MEMBER:
					label.Text = "Teilnehmer wählen: ";
					Picker memberpicker = new Picker();
					IReadOnlyList<Member> memberlist = participant.GetLagerClient().Members;
					foreach (Member mem in memberlist)
					{
						memberpicker.Items.Add(mem.ToString());
					}
					memberpicker.SelectedIndexChanged += (sender, args) =>
					{
						Member m = participant.GetLagerClient().GetMemberFromString(memberpicker.Items[memberpicker.SelectedIndex]);
						participant.Name = m.Display;
					};
					memberpicker.SelectedIndex = 0;
					manip = memberpicker;
					break;
				case GROUP:
					label.Text = "Gruppenname: ";
					manip = new Entry();
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
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(participant.GetLagerClient().Manager, participant.GetLagerClient());
			context.PacketId = new PacketId(participant.GetLagerClient().OwnCollaborator);
			await participant.OnSaveEditing(participant.GetLagerClient().ClientSerialiser, context, oldParticipant);
			await Navigation.PopModalAsync(true);
		}
	}
}
