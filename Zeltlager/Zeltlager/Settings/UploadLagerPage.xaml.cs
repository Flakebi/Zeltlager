using System;

using Xamarin.Forms;
using Zeltlager.Client;

namespace Zeltlager.Settings
{
	public partial class UploadLagerPage : ContentPage
	{
		LagerClient lager;

		public UploadLagerPage(LagerClient lager)
		{
			InitializeComponent();
			NavigationPage.SetBackButtonTitle(this, "");
			this.lager = lager;
		}

		async void OnUploadClicked(object sender, EventArgs e)
		{
			LoadingScreen ls = new LoadingScreen();
			await Navigation.PushModalAsync(new NavigationPage(ls), false);

			try
			{
				if (string.IsNullOrEmpty(lager.ClientManager.Settings.ServerAddress))
				{
					await DisplayAlert("Achtung!", "Bitte eine Serveradresse angeben.", "Ok");
				}
				else
				{
					await lager.Synchronise(status => ls.Status = status.GetMessage());
					await DisplayAlert("Hochladen erfolgreich", "Das aktuelle Lager wurde erfolgreich hochgeladen.", "Ok");
				}
			}
			catch (Exception ex)
			{
				await LagerManager.Log.Exception("Uploading Lager", ex);
				await DisplayAlert("Fehler", "Beim Hochladen des Lagers ist ein Fehler aufgetreten", "Ok");
			}

			await Navigation.PopModalAsync(false);
		}

	}
}
