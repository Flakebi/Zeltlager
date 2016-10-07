using System.Collections.Generic;
using Xamarin.Forms;

namespace Zeltlager
{
	public partial class SearchableListView : CustomListView<T>
	{
		IReadOnlyList<T> currentItems;
		IReadOnlyList<T> totalItems;

		public SearchableListView(List<T> itemsToDisplay)
		{
			InitializeComponent();
			totalItems = itemsToDisplay;
			// display everything at the beginning
			currentItems = totalItems;
			items.ItemsSource = currentItems;
		}
	}
}
