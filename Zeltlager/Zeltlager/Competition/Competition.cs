using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
    public class Competition : IStorable
    {
        Lager lager;

        public Competition(Lager lager)
        {
            this.lager = lager;
        }
    }
}
