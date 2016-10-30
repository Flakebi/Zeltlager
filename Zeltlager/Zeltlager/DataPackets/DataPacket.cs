using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	using Serialisation;

	/// <summary>
	/// A subclass of this type must have a default constructor and
	/// can optionally have the static method IdCount which says how many
	/// ids should be reserved for the type:
	/// public static int GetIdCount()
	/// All possible packet types have to be added to the packetTypes array.
	/// </summary>
	public abstract class DataPacket
	{
		static readonly Type[] packetTypes = {
			typeof(AddCollaborator),
			typeof(AddPacket),
			//TODO Convert calendar events
			typeof(AddCalendarEvent),
			typeof(DeleteCalendarEvent),
		};

		/// <summary>
		/// Read a packet. This function can throw an IOException, e.g. if the packet type is invalid.
		/// </summary>
		/// <param name="input">The input reader</param>
		/// <returns>The read packet.</returns>
		public static DataPacket ReadPacket(PacketId id, byte[] data)
		{
			using (BinaryReader input = new BinaryReader(new MemoryStream(data)))
			{
				int packetType = input.ReadInt32();
				int idCount = 0;
				for (int i = 0; i < packetTypes.Length; i++)
				{
					idCount = 1;
					// Check if the packet type specifies an id count
					var func = packetTypes[i].GetTypeInfo().GetDeclaredMethod("GetIdCount");
					if (func != null && func.IsStatic && func.GetParameters().Length == 0 && func.ReturnType == typeof(int))
						idCount = (int)func.Invoke(null, new object[0]);
					if (packetType < idCount)
						break;
					packetType -= idCount;
				}
				// Invalid id
				if (packetType >= idCount)
					// Create a new InvalidDataPacket
					return new InvalidDataPacket(data);

				// Create a new packet of the specified type using the default constructor
				DataPacket packet = (DataPacket)packetTypes[packetType].GetTypeInfo().DeclaredConstructors
					.First(ctor => ctor.GetParameters().Length == 0).Invoke(new object[0]);

				// Fill the packet data
				packet.Timestamp = DateTime.FromBinary(input.ReadInt64());
				packet.subId = idCount;
				packet.Data = input.ReadBytes((int)(input.BaseStream.Length - input.BaseStream.Position));
				packet.Id = id;
				return packet;
			}
		}

		/// <summary>
		/// Only set when available.
		/// </summary>
		public byte[] Signature { get; set; }
		public byte[] Iv { get; set; }

		public PacketId Id { get; private set; }
		/// <summary>
		/// The timestamp in UTC.
		/// </summary>
		public DateTime Timestamp { get; private set; }
		protected byte[] Data { get; set; }
		/// <summary>
		/// The sub-id of this packet.
		/// </summary>
		protected int subId { get; set; }

		protected DataPacket()
		{
			Timestamp = DateTime.UtcNow;
			subId = 0;
		}

		public void WritePacket(BinaryWriter output)
		{
			// Don't write the header for InvalidDataPackets.
			if (!(this is InvalidDataPacket))
			{
				// Write the type of this packet
				int packetType = 0;
				int idCount = 0;
				Type ownType = GetType();
				for (int i = 0; i < packetTypes.Length; i++)
				{
					if (packetTypes[i] == ownType)
						break;
					idCount = 1;
					// Check if the packet type specifies an id count
					var func = packetTypes[i].GetTypeInfo().GetDeclaredMethod("GetIdCount");
					if (func != null && func.IsStatic && func.GetParameters().Length == 0 && func.ReturnType == typeof(int))
						idCount = (int)func.Invoke(null, new object[0]);

					packetType += idCount;
				}
				idCount += subId;

				output.Write(idCount);
				output.Write(Timestamp.ToBinary());
			}
			output.Write(Data);
		}

		/// <summary>
		/// Applies the content of this packet to a lager.
		/// The packet has to deserialise itself from Data.
		/// </summary>
		/// <param name="lager">The lager to which this packet should be applied.</param>
		public abstract Task Deserialise(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context);
	}
}
