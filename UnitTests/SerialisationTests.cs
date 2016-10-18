using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Zeltlager;
using Zeltlager.Serialisation;

namespace UnitTests
{
	[TestFixture]
	public class SerialisationTests
	{
		[Test]
		public void SerialiseMember()
		{
			Collaborator collaborator = new Collaborator(0, new KeyPair(new byte[0], new byte[0], new byte[0]));
			Tent tent = new Tent(new TentId(collaborator, 0), 0, "Tent", false, new List<Member>());
			Member member = new Member(new MemberId(collaborator, 0), "Member", tent, true);
			LagerSerialisationContext context = new LagerSerialisationContext(null, collaborator);
			Serialiser<LagerSerialisationContext> ser = new Serialiser<LagerSerialisationContext>();
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				ser.Write(output, context, tent);
			}

			mem = new MemoryStream(mem.ToArray());
			using (BinaryReader input = new BinaryReader(mem))
			{
				Tent t = new Tent();
				ser.Read(input, context, t);
			}
		}
	}
}
