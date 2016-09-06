using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Zeltlager.DataPackets
{
	/// <summary>
	/// A subclass of this type must have a default constructor.
	/// All possible packet types have to be added to the packetTypes array.
	/// </summary>
	public abstract class DataPacket
	{
		/// <summary>
		/// The version of the packet protocol.
		/// </summary>
		static uint VERSION = 0;

		static Type[] packetTypes = {
			typeof(AddMemberPacket),
			typeof(DeleteMemberPacket),
			typeof(AddTentPacket),
			typeof(DeleteTentPacket),
			typeof(AddSupervisorToTentPacket),
			typeof(DeleteSupervisorFromTentPacket),
		};

		/// <summary>
		/// Read a packet. This function can throw an IOException, e.g. if the packet type is invalid.
		/// </summary>
		/// <param name="input">The input reader</param>
		/// <returns>The read packet.</returns>
		public static DataPacket ReadDataPacket(BinaryReader input)
		{
			byte packetType = input.ReadByte();

			if (packetType >= packetTypes.Length)
			{
				// Create a new InvalidDataPacket
				byte[] data = new byte[1 + input.BaseStream.Length - input.BaseStream.Position];
				data[0] = packetType;
				input.Read(data, 1, data.Length - 1);
				return new InvalidDataPacket(data);
			}

			// Create a new packet of the specified type using the default constructor
			DataPacket packet = (DataPacket)packetTypes[packetType].GetTypeInfo().DeclaredConstructors
				.First(ctor => ctor.GetParameters().Length == 0).Invoke(new object[0]);

			// Fill the packet data
			packet.Timestamp = DateTime.FromBinary(input.ReadInt64());
			packet.Data = new byte[input.BaseStream.Length - input.BaseStream.Position];
			input.Read(packet.Data, 0, packet.Data.Length);
			return packet;
		}

		/// <summary>
		/// Only set when available.
		/// </summary>
		public byte[] Signature { get; set; }
		public byte[] Iv { get; set; }

		public DateTime Timestamp { get; private set; }
		protected byte[] Data { get; set; }

		public DataPacket()
		{
			Timestamp = new DateTime();
		}

		public void WritePacket(BinaryWriter output)
		{
			// Don't write the header for InvalidDataPackets.
			if (!(this is InvalidDataPacket))
			{
				// Write the type of this packet
				var index = Array.IndexOf(packetTypes, GetType());
				if (index == -1)
					throw new InvalidOperationException("Trying to write an unknown packet type, you should add this packet to the DataPacket.packetTypes array.");
				output.Write((byte)index);
				output.Write(Timestamp.ToBinary());
			}
			output.Write(Data);
		}

		/// <summary>
		/// Applies the content of this packet to a lager.
		/// The packet has to deserialise itself from Data.
		/// </summary>
		/// <param name="lager">The lager to which this packet should be applied.</param>
		public abstract void Deserialise(Lager lager);
	}
}
