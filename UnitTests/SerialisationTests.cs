using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Zeltlager;
using Zeltlager.Client;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;

namespace UnitTests
{
    [TestFixture]
    public class SerialisationTests
    {
        [Test]
        public void SerialiseMember()
        {
            Collaborator collaborator = new Collaborator(0, new KeyPair());
            Tent tent = new Tent(new TentId(collaborator, 0), 0, "Tent", false, new List<Member>());
            Member member = new Member(new MemberId(collaborator, 0), "Member", tent, true);
            SerialisationContext context = new SerialisationContext(null, null);
            Serialiser ser = new Serialiser();
            MemoryStream mem = new MemoryStream();
            using (BinaryWriter output = new BinaryWriter(mem))
            {
                ser.Write(output, context, member);
            }

            mem = new MemoryStream(mem.ToArray());
            using (BinaryReader input = new BinaryReader(mem))
            {
                Member m = ser.Read<Member>(input, context);
                //Assert.Equals(member, m);
            }
        }
    }
}
