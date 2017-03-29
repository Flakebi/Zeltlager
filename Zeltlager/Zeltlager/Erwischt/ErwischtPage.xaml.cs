﻿using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.Client;
using System.Linq;
using System.Threading.Tasks;
using Zeltlager.UAM;

namespace Zeltlager.Erwischt
{
	public partial class ErwischtPage : ContentPage
	{
		ErwischtGame game;
		LagerClient lager;

		public ErwischtPage(ErwischtGame game, LagerClient lager)
		{
			InitializeComponent();
			this.game = game;
			this.lager = lager;
			Title = game.Name;
		}

		void UpdateUI()
		{
			Content = new SearchableListView<ErwischtParticipant>(game.ErwischtParticipants,
															 OnEditClicked, OnDeleteClicked, OnErwischtParticipantClicked);
		}

		void OnErwischtParticipantClicked(ErwischtParticipant member)
		{
			Navigation.PushAsync(new ErwischtParticipantDetailPage(member));
		}

		void OnEditClicked(ErwischtParticipant member) { }

		async Task OnDeleteClicked(ErwischtParticipant member) { await Task.WhenAll(); }

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new UniversalAddModifyPage<ErwischtGame, ErwischtGame>(new ErwischtGame("", lager), true, lager));
		}

		void OnChangeGameButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new ChangeErwischtGamePage(lager));
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}
	}
}
