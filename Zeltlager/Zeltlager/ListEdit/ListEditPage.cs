using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Zeltlager
{
	public class ListEditPage<T> : ContentPage, INotifyPropertyChanged where T : ISearchable
	{
		public List<T> SelectedItems
		{
			get { return new List<T> (selectedItems); }
			set { selectedItems = new ObservableCollection<T>(value); }
		}

		private ObservableCollection<T> selectedItems;

		ObservableCollection<T> unselectedItems;

		List<T> saveCollectedItems;

		ListView selectedItemsView, unselectedItemsView;

		public ListEditPage(List<T> toChooseFrom, List<T> choosenOnes, string header)
		{
			Title = header;
			saveCollectedItems = choosenOnes;

			if (choosenOnes == null)
				selectedItems = new ObservableCollection<T>();
			else
				selectedItems = new ObservableCollection<T>(choosenOnes);
			unselectedItems = new ObservableCollection<T>(toChooseFrom.Except(SelectedItems));

			var vsl = new StackLayout
			{
				Orientation = StackOrientation.Vertical
			};

			var dataTemplate = new DataTemplate(typeof(TextCell));
			dataTemplate.SetBinding(TextCell.TextProperty, new Binding("SearchableText"));
			dataTemplate.SetBinding(TextCell.DetailProperty, new Binding("SearchableDetail"));

			selectedItemsView = new ListView
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.Start,
				ItemsSource = selectedItems,
				ItemTemplate = dataTemplate,
				BindingContext = selectedItems
			};
			selectedItemsView.ItemSelected += OnSelectedItemClicked;

			unselectedItemsView = new ListView
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.EndAndExpand,
				ItemsSource = unselectedItems,
				ItemTemplate = dataTemplate,
				BindingContext = unselectedItems
			};
			unselectedItemsView.ItemSelected += OnUnselectedItemClicked;

			vsl.Children.Add(selectedItemsView);
			vsl.Children.Add(unselectedItemsView);
			Content = vsl;

			ToolbarItems.Add(new ToolbarItem(null, Icons.CANCEL, OnCancelClicked, ToolbarItemOrder.Primary, 0));
			ToolbarItems.Add(new ToolbarItem(null, Icons.SAVE, OnSaveClicked, ToolbarItemOrder.Primary, 1));
			Style = (Style)Application.Current.Resources["BaseStyle"];
			// make page not start directly at the top
			Padding = new Thickness(10);
			NavigationPage.SetBackButtonTitle(this, "");
		}

		void OnUnselectedItemClicked(object sender, SelectedItemChangedEventArgs e)
		{
			var item = (T) e.SelectedItem;
			unselectedItems.Remove(item);
			selectedItems.Add(item);
			OnPropertyChanged(nameof(SelectedItems));
			OnPropertyChanged(nameof(unselectedItems));
		}

		void OnSelectedItemClicked(object sender, SelectedItemChangedEventArgs e)
		{
			var item = (T)e.SelectedItem;
			selectedItems.Remove(item);
			unselectedItems.Add(item);
			OnPropertyChanged(nameof(SelectedItems));
		}

		void OnCancelClicked()
		{
			Navigation.PopModalAsync(true);
		}

		void OnSaveClicked()
		{
			// somehow return selectedItems
			Navigation.PopModalAsync(true);
			saveCollectedItems.Clear();
			saveCollectedItems.AddRange(selectedItems);
		}

		#region Interface implementation

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}

