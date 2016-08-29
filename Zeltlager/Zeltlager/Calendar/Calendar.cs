using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.Calendar
{
    public class Calendar : IStorable
    {
        Lager lager;

        public Calendar(Lager lager)
        {
            this.lager = lager;
        }
    }
}
