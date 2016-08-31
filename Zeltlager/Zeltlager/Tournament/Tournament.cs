using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.Tournament
{
    public class Tournament : ILagerPart
    {
        Lager lager;

        public Tournament(Lager lager)
        {
            this.lager = lager;
        }
    }
}
