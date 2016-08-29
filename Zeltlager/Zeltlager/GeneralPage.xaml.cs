using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Zeltlager
{
    public partial class GeneralPage : ContentPage
    {
        public GeneralPage()
        {
            InitializeComponent();
        }

        void OnMemberClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MembersPage());
        }

        void OnTentClicked(object sender, EventArgs e)
        {
        }
    }
}
