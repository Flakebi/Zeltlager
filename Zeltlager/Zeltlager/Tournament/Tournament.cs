using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.Tournament
{
    class Tournament : IStorable
    {
        Zeltlager zeltlager;

        public Tournament(Zeltlager zeltlager)
        {
            this.zeltlager = zeltlager;
        }
    }
}
