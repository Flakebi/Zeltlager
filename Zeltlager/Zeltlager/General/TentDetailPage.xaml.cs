using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.Client;
using System.Linq;
namespace Zeltlager.General
{
	public partial class TentDetailPage : ContentPage
	{
		//Tent tent;

		public TentDetailPage(Tent tent, LagerClient lager)
		{
			InitializeComponent();
			Title = tent.Display;
			Padding = new Thickness(10);

			Grid grid = new Grid();
			grid.RowSpacing = 0;
			RowDefinition rd = new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) };
			RowDefinition rdfill = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
			RowDefinition rdlist = new RowDefinition { Height = new GridLength(150, GridUnitType.Absolute) };

			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

			grid.RowDefinitions.Add(rd);
			grid.Children.Add(new Label
			{
				Text = "Zeltname: ",
				TextColor = (Color)Application.Current.Resources["textColorSecondary"]
			}, 0, 0);
			grid.Children.Add(new Label { Text = tent.Name }, 1, 0);

			grid.RowDefinitions.Add(rd);
			grid.Children.Add(new Label
			{
				Text = "Zeltnummer: ",
				TextColor = (Color)Application.Current.Resources["textColorSecondary"]
			}, 0, 1);
			grid.Children.Add(new Label { Text = tent.Number.ToString() }, 1, 1);

			grid.RowDefinitions.Add(rd);
			grid.Children.Add(new Label
			{
				Text = "Geschlecht: ",
				TextColor = (Color)Application.Current.Resources["textColorSecondary"]
			}, 0, 2);
			grid.Children.Add(new Label { Text = tent.Girls ? "♀" : "♂" }, 1, 2);


			grid.RowDefinitions.Add(rd);
			grid.Children.Add(new Label
			{
				Text = "Zeltbetreuer: ",
				TextColor = (Color)Application.Current.Resources["textColorSecondary"]
			}, 0, 2, 3, 4);

			grid.RowDefinitions.Add(rdlist);
			grid.Children.Add(new SearchableListView<Member>(tent.Supervisors.Where(m => m.IsVisible).ToList(), null, null, null), 0, 2, 4, 5);


			grid.RowDefinitions.Add(rd);
			grid.Children.Add(new Label
			{
				Text = "Zeltbewohner: ",
				TextColor = (Color)Application.Current.Resources["textColorSecondary"]
			}, 0, 2, 5, 6);

			grid.RowDefinitions.Add(rdfill);
			grid.Children.Add(new SearchableListView<Member>(tent.GetMembers().Where(m => m.IsVisible).ToList(), null, null, null), 0, 2, 6, 7);

			var scrollView = new ScrollView();
			scrollView.Content = grid;

			Content = scrollView;
			NavigationPage.SetBackButtonTitle(this, "");
		}
	}
}
