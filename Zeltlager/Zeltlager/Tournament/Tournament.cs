﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.Tournament
{
    public class Tournament : IStorable
    {
        Lager lager;

        public Tournament(Lager lager)
        {
            this.lager = lager;
        }
    }
}
