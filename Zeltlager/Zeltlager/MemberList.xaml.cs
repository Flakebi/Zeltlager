using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

namespace Zeltlager
{
	public partial class MemberList : ContentView
	{
		IReadOnlyList<Member> currentMembers;

		public MemberList()
		{
			InitializeComponent();
			currentMembers = Lager.CurrentLager.Members;
			members.ItemsSource = currentMembers;
		}

		void OnSearch(object sender, TextChangedEventArgs e)
		{
			// Filter the memberlist
			List<Member> sourceList = new List<Member>(Lager.CurrentLager.Members);
			sourceList.Sort();
			List<Member> currentMembers = new List<Member>();
			var filters = e.NewTextValue.Split(' ');

			// Reverse filters so the last processed filter is the most important
			foreach (var filterIter in filters.Reverse())
			{
				// Ignore empty strings, that happens e.g. when the filter text is empty
				if (string.IsNullOrEmpty(filterIter))
					continue;

				var filter = filterIter.ToLowerInvariant();

				// Name starts with the filter text
				for (int i = 0; i < sourceList.Count; i++)
				{
					Member m = sourceList[i];
					if (m.Display.ToLowerInvariant().StartsWith(filter))
					{
						sourceList.RemoveAt(i);
						currentMembers.Add(m);
						i--;
					}
				}
				// Name contains the filter text
				for (int i = 0; i < sourceList.Count; i++)
				{
					Member m = sourceList[i];
					if (m.Display.ToLowerInvariant().Contains(filter))
					{
						sourceList.RemoveAt(i);
						currentMembers.Add(m);
						i--;
					}
				}
				// Tent starts with the filter text
				for (int i = 0; i < sourceList.Count; i++)
				{
					Member m = sourceList[i];
					if (m.Tent.Display.ToLowerInvariant().StartsWith(filter))
					{
						sourceList.RemoveAt(i);
						currentMembers.Add(m);
						i--;
					}
				}
				// Tent contains the filter text
				for (int i = 0; i < sourceList.Count; i++)
				{
					Member m = sourceList[i];
					if (m.Tent.Display.ToLowerInvariant().Contains(filter))
					{
						sourceList.RemoveAt(i);
						currentMembers.Add(m);
						i--;
					}
				}

				// Switch lists so all search words have to be contained in the list
				sourceList = currentMembers;
				currentMembers = new List<Member>();
			}

			this.currentMembers = sourceList;
			members.ItemsSource = this.currentMembers;
		}
	}
}
