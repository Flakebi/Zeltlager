using System;
using Xamarin.Forms;
using Zeltlager.General;
using Zeltlager.Client;
using Zeltlager.Erwischt;
using Zeltlager.UAM;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	public partial class MainPage : ContentPage
	{
		LagerClient lager;

		public MainPage(LagerClient lager)
		{
			InitializeComponent();
			this.lager = lager;
			NavigationPage.SetBackButtonTitle(this, "");
			BindingContext = this;
		}

		public string LagerName => lager.Data.Name;

		async void OnSynchronizeClicked(object sender, EventArgs e)
		{
			LoadingScreen ls = new LoadingScreen();
			await Navigation.PushModalAsync(new NavigationPage(ls), false);

			//Task rotation = ViewExtensions.RelRotateTo(syncButton, 360*2, 4000*2, Easing.CubicInOut);

			if (string.IsNullOrEmpty(lager.ClientManager.Settings.ServerAddress))
			{
				await DisplayAlert("Achtung!", "Bitte eine Serveradresse angeben.", "Ok");
			}
			else 
			{
				try
				{
					//await lager.Synchronise(status => syncButton.Text = status.GetMessage());
					await lager.Synchronise(status => ls.Status = status.GetMessage());
				}
				catch (Exception ex)
				{
					await LagerManager.Log.Exception("Synchronize lager", ex);
					await DisplayAlert("Fehler", "Beim Synchronisieren des Lagers ist ein Fehler aufgetreten.", "Ok");
				}
			}
			//ViewExtensions.CancelAnimations(syncButton);
			await Navigation.PopModalAsync(false);
		}

		void OnCompetitionClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Competition.CompetitionHandlerPage(lager));
		}

		void OnErwischtClicked(object sender, EventArgs e)
		{
			if (lager.ErwischtHandler.VisibleGames.Any())
			{
				lager.ErwischtHandler.CurrentGame = lager.ErwischtHandler.GetNewestGame();
				Navigation.PushAsync(new ErwischtPage(lager.ErwischtHandler.GetNewestGame(), lager));
			}
			else
			{
				Navigation.PushAsync(new UniversalAddModifyPage<ErwischtGame, ErwischtGame>
																			(new ErwischtGame("", lager), true, lager));
			}
		}

		void OnCalendarClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Calendar.CalendarPage(lager));
		}

		void OnGeneralClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new GeneralPage(lager));
		}

		void OnSettingsClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Settings.SettingsPage(lager));
		}
	}
}
