using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.Calendar
{
    class Calendar : IStorable
    {
        Zeltlager zeltlager;

        public Calendar(Zeltlager zeltlager)
        {
            this.zeltlager = zeltlager;
        }
    }
}
