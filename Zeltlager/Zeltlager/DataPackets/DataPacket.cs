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
		static Type[] packetTypes = {
			typeof(Bundle),
			typeof(AddMember),
			typeof(DeleteMember),
			typeof(AddTent),
			typeof(DeleteTent),
			typeof(AddSupervisorToTent),
			typeof(DeleteSupervisorFromTent),
			typeof(AddCalendarEvent),
			typeof(DeleteCalendarEvent),
		};

		/// <summary>
		/// Read a packet. This function can throw an IOException, e.g. if the packet type is invalid.
		/// </summary>
		/// <param name="input">The input reader</param>
		/// <returns>The read packet.</returns>
		public static DataPacket ReadPacket(byte[] input)
		{
			byte packetType = input[0];

			if (packetType >= packetTypes.Length)
			{
				// Create a new InvalidDataPacket
				return new InvalidDataPacket(input);
			}

			// Create a new packet of the specified type using the default constructor
			DataPacket packet = (DataPacket)packetTypes[packetType].GetTypeInfo().DeclaredConstructors
				.First(ctor => ctor.GetParameters().Length == 0).Invoke(new object[0]);

			// Fill the packet data
			packet.Timestamp = DateTime.FromBinary(input.ToLong(1));
			packet.Data = new byte[input.Length - 9];
			Array.Copy(input, 9, packet.Data, 0, packet.Data.Length);
			return packet;
		}

		/// <summary>
		/// Only set when available.
		/// </summary>
		public byte[] Signature { get; set; }
		public byte[] Iv { get; set; }

		/// <summary>
		/// The timestamp in UTC.
		/// </summary>
		public DateTime Timestamp { get; private set; }
		protected byte[] Data { get; set; }

		public DataPacket()
		{
			Timestamp = DateTime.UtcNow;
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
				output.Write(Timestamp.ToBinary().ToBytes());
			}
			output.Write(Data);
		}

		/// <summary>
		/// Applies the content of this packet to a lager.
		/// The packet has to deserialise itself from Data.
		/// </summary>
		/// <param name="lager">The lager to which this packet should be applied.</param>
		public abstract void Deserialise(Client.LagerClient lager, Collaborator collaborator);
	}
}
