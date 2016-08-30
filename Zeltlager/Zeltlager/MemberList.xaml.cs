using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			string filter = e.NewTextValue;

			// Name starts with the filter text
			for (int i = 0; i < sourceList.Count; i++)
			{
				Member m = sourceList[i];
				if (m.Display.StartsWith(filter))
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
				if (m.Display.Contains(filter))
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
				if (m.Tent.Display.StartsWith(filter))
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
				if (m.Tent.Display.Contains(filter))
				{
					sourceList.RemoveAt(i);
					currentMembers.Add(m);
					i--;
				}
			}

			this.currentMembers = currentMembers;

			if (Device.OS == TargetPlatform.Windows)
				// At the moment, it will crash on windows if the list is empty
				members.ItemsSource = currentMembers.DefaultIfEmpty();
			else
				members.ItemsSource = currentMembers;

		}
	}
}
