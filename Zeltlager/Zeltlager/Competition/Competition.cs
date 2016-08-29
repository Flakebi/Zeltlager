using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
    class Competition : IStorable
    {
        Zeltlager zeltlager;

        public Competition(Zeltlager zeltlager)
        {
            this.zeltlager = zeltlager;
        }
    }
}
