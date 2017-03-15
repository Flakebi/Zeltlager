using System.IO;

using NUnit.Framework;

using Zeltlager.CommunicationPackets;
using Requests = Zeltlager.CommunicationPackets.Requests;
using Responses = Zeltlager.CommunicationPackets.Responses;

namespace UnitTests
{
	[TestFixture]
	public class CommunicationPacketTests
	{
		[Test]
		public void RegisterResponse()
		{
			int collaboratorId = 42;
			var packet = new Responses.Register(collaboratorId);
			
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
				packet.WritePacket(output);
			
			var result = (Responses.Register)CommunicationPacket.ReadPacket(mem.ToArray());
			Assert.AreEqual(collaboratorId, result.GetCollaboratorId());
		}
	}
}
