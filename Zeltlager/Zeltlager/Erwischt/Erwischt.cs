using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.Erwischt
{
    /// <summary>
    /// Data associated to each member for the Erwischt game.
    /// </summary>
    class MemberData
    {
        /// <summary>
        /// If this member is still alive.
        /// </summary>
        public bool Alive;
        /// <summary>
        /// The next target of this member.
        /// </summary>
        public Member Target;
    }

    class Erwischt : IStorable
    {
        Zeltlager zeltlager;
        Dictionary<Member, MemberData> memberData;

        public Erwischt(Zeltlager zeltlager)
        {
            this.zeltlager = zeltlager;
        }
    }
}
