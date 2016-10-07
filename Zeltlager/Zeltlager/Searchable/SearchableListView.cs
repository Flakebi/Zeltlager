﻿using System;

using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Zeltlager
{
	public class SearchableListView<T> : ContentView where T : ISearchable
	{
		IReadOnlyList<T> currentItems;
		IReadOnlyList<T> totalItems;
		ListView listView;

		public SearchableListView(IReadOnlyList<T> items)
		{
			totalItems = items;
			// display everything at the beginning
			currentItems = totalItems;

			// build Layout
			StackLayout stackLayout = new StackLayout
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Orientation = StackOrientation.Vertical,
				Spacing = 15
			};
			SearchBar searchBar = new SearchBar();
			searchBar.TextChanged += OnSearch;
			stackLayout.Children.Add(searchBar);

			listView = new ListView();
			var dataTemplate = new DataTemplate(typeof(TextCell));
			dataTemplate.SetBinding(TextCell.TextProperty, new Binding("SearchableText"));
			dataTemplate.SetBinding(TextCell.DetailProperty, new Binding("SearchableDetail"));
			listView.ItemTemplate = dataTemplate;
			listView.BindingContext = items;
			listView.ItemsSource = currentItems;
			stackLayout.Children.Add(listView);

			Content = stackLayout;
			Style = (Style)Application.Current.Resources["BaseStyle"];
		}

		void OnSearch(object sender, TextChangedEventArgs e)
		{
			// set current items to only show ones matching to the search text
			List<T> newTotalItems = new List<T>(totalItems);
			newTotalItems.Sort();
			List<T> newCurrentItems = new List<T>();
			// get the words to filter for
			var filters = e.NewTextValue.Split(' ');

			// go through totalItems and remove everything not conforming to all search tags
			// reverse list, so most result is sorted for most important tag last
			foreach (var filter in filters.Reverse())
			{
				// ignore empty tags
				if (string.IsNullOrEmpty(filter))
					continue;

				var filterTag = filter.ToLowerInvariant();

				// Text starts with the filter tag
				FilterForCondition(newTotalItems, newCurrentItems, t => t.SearchableText.StartsWith(filterTag, StringComparison.OrdinalIgnoreCase));

				// Text contains the filter tag
				FilterForCondition(newTotalItems, newCurrentItems, t => t.SearchableText.ToLowerInvariant().Contains(filterTag));

				// Detail text starts with the filter tag
				FilterForCondition(newTotalItems, newCurrentItems, t => t.SearchableDetail.StartsWith(filterTag, StringComparison.OrdinalIgnoreCase));

				// Detail text contains the filter tag
				FilterForCondition(newTotalItems, newCurrentItems, t => t.SearchableDetail.ToLowerInvariant().Contains(filterTag));

				// switch lists so next round we only search in the list of things conforming to the current tag (and all old ones)
				newTotalItems = newCurrentItems;
				newCurrentItems = new List<T>();
			}
			// fill displayed list completely filtered items
			currentItems = newTotalItems;
			listView.ItemsSource = currentItems;
		}

		static void FilterForCondition(List<T> oldList, List<T> newList, Func<T, bool> condition)
		{
			for (int i = 0; i < oldList.Count; i++)
			{
				T t = oldList[i];
				if (condition(t))
				{
					oldList.RemoveAt(i);
					newList.Add(t);
					i--;
				}
			}
		}
	}
}

