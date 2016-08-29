using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager
{
    class Member
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public Tent Tent { get; set; }
        public bool Supervisor { get; set; }

        public Member()
        {
        }

        public Member(uint id, string name, Tent tent, bool supervisor)
        {
            Id = id;
            Name = name;
            Tent = tent;
            Supervisor = supervisor;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
