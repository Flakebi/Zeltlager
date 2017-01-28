using System;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.CommunicationPackets.Requests
{
	using Client;
	using Network;

    public class AddLager : CommunicationRequest
    {
		AddLager() { }

		public AddLager(LagerClient lager)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(lager.Data);
			}
			Data = mem.ToArray();
		}

		public override async Task Apply(INetworkConnection connection, LagerManager manager)
		{
			// Verify the request and add the lager
			bool success = false;
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				LagerData data = input.ReadLagerData();
				try
				{
					await data.Verify();
					var lager = await manager.AddLager(data);
					// Send the created lager id back
					await connection.WritePacket(new Responses.AddLager(lager.Id));
					success = true;
				}
				catch (Exception e)
				{
					await LagerManager.Log.Exception("Add lager", e);
				}
			}
			if (!success)
				await connection.WritePacket(new Responses.Status(false));
		}
	}
}
