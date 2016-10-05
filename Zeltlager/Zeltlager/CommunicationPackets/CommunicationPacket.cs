using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Zeltlager.CommunicationPackets
{
	/// <summary>
	/// A subclass of this type must have a default constructor.
	/// All possible packet types have to be added to the packetTypes array.
	/// </summary>
	public abstract class CommunicationPacket
	{
		/// <summary>
		/// The version of the data packet protocol.
		/// </summary>
		static uint VERSION = 0;

		static Type[] packetTypes = {
			typeof(Requests.ListGames),
			typeof(Responses.ListGames),
		};

		/// <summary>
		/// Read a packet. This function can throw an IOException, e.g. if the packet type is invalid.
		/// </summary>
		/// <param name="packetTypes">A list of possible packet types</param>
		/// <param name="input">The input reader</param>
		/// <returns>The read packet.</returns>
		protected static CommunicationPacket ReadPacket(BinaryReader input)
		{
			byte packetType = input.ReadByte();

			if (packetType >= packetTypes.Length)
				throw new IOException("Invalid packet type");

			// Create a new packet of the specified type using the default constructor
			CommunicationPacket packet = (CommunicationPacket)packetTypes[packetType].GetTypeInfo().DeclaredConstructors
				.First(ctor => ctor.GetParameters().Length == 0).Invoke(new object[0]);

			// Fill the packet data
			packet.Data = new byte[input.BaseStream.Length - input.BaseStream.Position];
			input.Read(packet.Data, 0, packet.Data.Length);
			return packet;
		}

		protected byte[] Data { get; set; }

		public CommunicationPacket()
		{
		}

		protected void WritePacket(BinaryWriter output)
		{
			var index = Array.IndexOf(packetTypes, GetType());
			if (index == -1)
				throw new InvalidOperationException("Trying to write an unknown packet type, you should add this packet to the CommunicationPacket.packetTypes array.");
			output.Write((byte)index);
			output.Write(Data);
		}
	}
}
