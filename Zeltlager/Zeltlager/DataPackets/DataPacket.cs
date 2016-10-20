using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Zeltlager.DataPackets
{
	using Serialisation;

	/// <summary>
	/// A subclass of this type must have a default constructor and
	/// can optionally have the static method IdCount which says how many
	/// ids should be reserved for the type:
	/// public static byte GetIdCount()
	/// All possible packet types have to be added to the packetTypes array.
	/// </summary>
	public abstract class DataPacket
	{
		static readonly Type[] packetTypes = {
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
			byte idCount = 0;
			for (int i = 0; i < packetTypes.Length; i++)
			{
				idCount = 1;
				// Check if the packet type specifies an id count
				var func = packetTypes[i].GetTypeInfo().GetDeclaredMethod("GetIdCount");
				if (func != null && func.IsStatic && func.GetParameters().Length == 0 && func.ReturnType == typeof(byte))
					idCount = (byte)func.Invoke(null, new object[0]);
				if (packetType < idCount)
					break;
				else
					packetType -= idCount;
			}
			// Invalid id
			if (packetType >= idCount)
			{
				// Create a new InvalidDataPacket
				return new InvalidDataPacket(input);
			}

			// Create a new packet of the specified type using the default constructor
			DataPacket packet = (DataPacket)packetTypes[packetType].GetTypeInfo().DeclaredConstructors
				.First(ctor => ctor.GetParameters().Length == 0).Invoke(new object[0]);

			// Fill the packet data
			packet.Timestamp = DateTime.FromBinary(input.ToLong(1));
			packet.Id = idCount;
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
		/// <summary>
		/// The sub-id of this packet.
		/// </summary>
		protected byte Id { get; set; }

		public DataPacket()
		{
			Timestamp = DateTime.UtcNow;
			Id = 0;
		}

		public void WritePacket(BinaryWriter output)
		{
			// Don't write the header for InvalidDataPackets.
			if (!(this is InvalidDataPacket))
			{
				// Write the type of this packet
				byte packetType = 0;
				byte idCount = 0;
				Type ownType = GetType();
				for (int i = 0; i < packetTypes.Length; i++)
				{
					if (packetTypes[i] == ownType)
						break;
					idCount = 1;
					// Check if the packet type specifies an id count
					var func = packetTypes[i].GetTypeInfo().GetDeclaredMethod("GetIdCount");
					if (func != null && func.IsStatic && func.GetParameters().Length == 0 && func.ReturnType == typeof(byte))
						idCount = (byte)func.Invoke(null, new object[0]);

					packetType += idCount;
				}
				idCount += Id;

				output.Write(idCount);
				output.Write(Timestamp.ToBinary().ToBytes());
			}
			output.Write(Data);
		}

		/// <summary>
		/// Applies the content of this packet to a lager.
		/// The packet has to deserialise itself from Data.
		/// </summary>
		/// <param name="lager">The lager to which this packet should be applied.</param>
		/// <param name="packetType">The type id of the packet inside of this packet type.</param>
		public abstract void Deserialise(LagerClientSerialisationContext context, byte packetType);
	}
}
