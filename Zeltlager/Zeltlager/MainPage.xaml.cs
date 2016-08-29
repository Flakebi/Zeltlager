using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Zeltlager
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        void OnSynchronizeClicked(object sender, EventArgs e)
        {
        }

        void OnTournamentClicked(object sender, EventArgs e)
        {
        }

        void OnCompetitionClicked(object sender, EventArgs e)
        {
        }

        void OnErwischtClicked(object sender, EventArgs e)
        {
        }

        void OnCalendarClicked(object sender, EventArgs e)
        {
        }

        void OnGeneralClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new GeneralPage());
        }
    }
}
