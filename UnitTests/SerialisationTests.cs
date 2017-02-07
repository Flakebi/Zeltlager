using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Zeltlager;

namespace UnitTests
{
	[TestFixture]
    public class SerialisationTests : LagerTest
	{
		[Test]
		public void SerialiseTent()
		{
			Task.WaitAll(SerialiseTentAsync());
		}

		public async Task SerialiseTentAsync()
		{
			await Init();
            Tent tent = lager.Tents.First();
			
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
                await serialiser.Write(output, context, tent);
			}

			mem = new MemoryStream(mem.ToArray());
			using (BinaryReader input = new BinaryReader(mem))
			{
				Tent t = new Tent();
				await serialiser.Read(input, context, t);
				Assert.AreEqual(tent, t);
			}
		}

		[Test]
		public void SerialiseMember()
		{
			Task.WaitAll(SerialiseMemberAsync());
		}

		public async Task SerialiseMemberAsync()
		{
			await Init();
            Member member = lager.Members.First();

			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				await serialiser.Write(output, context, member);
			}

			mem = new MemoryStream(mem.ToArray());
			using (BinaryReader input = new BinaryReader(mem))
			{
				Member m = new Member(lager);
				await serialiser.Read(input, context, m);
				Assert.AreEqual(member, m);
			}
		}
	}
}
