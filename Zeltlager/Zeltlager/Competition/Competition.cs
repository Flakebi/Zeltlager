using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Client;

    public class Competition : ILagerPart
    {
        LagerClient lager;

        public Competition(LagerClient lager)
        {
            this.lager = lager;
        }
    }
}
