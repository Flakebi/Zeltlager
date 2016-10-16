using System.IO;
using System.IO.Compression;

namespace Zeltlager.DataPackets
{
	using Client;

    public class Bundle : DataPacket
    {
        public Bundle() { }

        public Bundle(DataPacket[] packets)
		{
			MemoryStream mem = new MemoryStream();
			// Compress the data using gzip
			using (BinaryWriter output = new BinaryWriter(new GZipStream(mem, CompressionLevel.Optimal)))
			{
				output.Write((ushort)packets.Length);
				foreach (var packet in packets)
				{
					MemoryStream tmp = new MemoryStream();
					using (BinaryWriter tmpOut = new BinaryWriter(tmp))
						packet.WritePacket(tmpOut);
					byte[] data = tmp.ToArray();
					output.Write(data.Length);
					output.Write(data);
				}
			}
			Data = mem.ToArray();
		}

		public DataPacket[] GetPackets()
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(new GZipStream(mem, CompressionMode.Decompress)))
			{
				ushort count = input.ReadUInt16();
				DataPacket[] packets = new DataPacket[count];
				for (ushort i = 0; i < count; i++)
				{
					int length = input.ReadInt32();
					byte[] data = input.ReadBytes(length);
					packets[i] = ReadPacket(data);
				}
				return packets;
			}
		}

		public override void Deserialise(LagerClient lager, Collaborator collaborator)
		{
			foreach (var packet in GetPackets())
				packet.Deserialise(lager, collaborator);
		}
    }
}
