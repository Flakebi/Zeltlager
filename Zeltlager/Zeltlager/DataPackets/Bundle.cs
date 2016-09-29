using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
    public class Bundle : DataPacket
    {
        public Bundle() { }

        public Bundle(IEnumerable<DataPacket> packets)
        {
        }

        public override void Deserialise(Lager lager)
        {
        }
    }
}
