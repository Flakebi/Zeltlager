using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager
{
    class Tent
    {
        public uint Number { get; set; }
        public string Name { get; set; }
        public List<Member> Supervisors { get; set; }

        public override string ToString()
        {
            return Number + " " + Name;
        }
    }
}
