using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager
{
    class Zeltlager : IStorable
    {
        public static Zeltlager CurrentZeltlager { get; set; }

        List<Member> members = new List<Member>();
        List<Tent> tents = new List<Tent>();

        // Subspaces
        public Tournament.Tournament Tournament { get; private set; }
        public Competition.Competition Competition { get; private set; }
        public Erwischt.Erwischt Erwischt { get; private set; }
        public Calendar.Calendar Calendar { get; private set; }

        public Zeltlager()
        {
            Tournament = new Tournament.Tournament(this);
            Competition = new Competition.Competition(this);
            Erwischt = new Erwischt.Erwischt(this);
            Calendar = new Calendar.Calendar(this);
        }

        private IStorable[] GetIStorables()
        {
            return new IStorable[]
            {
                this,
                Tournament,
                Competition,
                Erwischt,
                Calendar,
            };
        }
    }
}
