using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager
{
    public class Lager : IStorable
    {
        public static Lager CurrentLager { get; set; }
        
        List<Member> members = new List<Member>();
        List<Tent> tents = new List<Tent>();

        public IReadOnlyList<Member> Members { get { return members; } }
        public IReadOnlyList<Tent> Tents { get { return tents; } }

        // Subspaces
        public Tournament.Tournament Tournament { get; private set; }
        public Competition.Competition Competition { get; private set; }
        public Erwischt.Erwischt Erwischt { get; private set; }
        public Calendar.Calendar Calendar { get; private set; }

        public Lager()
        {
            Tournament = new Tournament.Tournament(this);
            Competition = new Competition.Competition(this);
            Erwischt = new Erwischt.Erwischt(this);
            Calendar = new Calendar.Calendar(this);

            members.Add(new Member(0, "Caro", new Tent(0, "Regenbogenforellen"), true));
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
