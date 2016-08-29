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
        public MemberList()
        {
            InitializeComponent();
            members.ItemsSource = Lager.CurrentLager.Members;
        }

        void OnSearch(object sender, TextChangedEventArgs e)
        {
            // Filter the memberlist
        }
    }
}
