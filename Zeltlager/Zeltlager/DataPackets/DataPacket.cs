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
	public abstract class DataPacket : IComparable<DataPacket>, ISearchable
	{
		static readonly Type[] packetTypes = {
			typeof(AddPacket),
			typeof(EditPacket),
			typeof(RevertPacket),
			typeof(ErwischtPacket),
			typeof(DeleteErwischtPacket),
		};

		/// <summary>
		/// Read a packet. This function can throw an IOException, e.g. if the packet type is invalid.
		/// </summary>
		/// <returns>The read packet.</returns>
		public static DataPacket ReadPacket(PacketId id, byte[] data)
		{
			using (BinaryReader input = new BinaryReader(new MemoryStream(data)))
			{
				int subId = input.ReadInt32();
				int idCount = 0;
				int packetType;
				for (packetType = 0; packetType < packetTypes.Length; packetType++)
				{
					idCount = 1;
					// Check if the packet type specifies an id count
					var func = packetTypes[packetType].GetTypeInfo().GetDeclaredMethod("GetIdCount");
					if (func != null && func.IsStatic && func.GetParameters().Length == 0 && func.ReturnType == typeof(int))
						idCount = (int)func.Invoke(null, new object[0]);
					if (subId < idCount)
						break;
					subId -= idCount;
				}
				// Invalid id
				if (subId >= idCount || subId < 0)
					// Create a new InvalidDataPacket
					return new InvalidDataPacket(data);

				// Create a new packet of the specified type using the default constructor
				DataPacket packet = (DataPacket)packetTypes[packetType].GetTypeInfo().DeclaredConstructors
					.First(ctor => ctor.GetParameters().Length == 0).Invoke(new object[0]);

				// Fill the packet data
				packet.Timestamp = DateTime.FromBinary(input.ReadInt64());
				packet.subId = subId;
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

		public PacketId Id { get; set; }
		/// <summary>
		/// The timestamp in UTC.
		/// </summary>
		public DateTime Timestamp { get; private set; }
		protected byte[] Data { get; set; }
		/// <summary>
		/// The sub-id of this packet.
		/// </summary>
		protected int subId { get; set; }

		/// <summary>
		/// A string description of this packet used in ToString.
		/// Packets should set this when they get deserialised.
		/// </summary>
		protected string contentString;

		/// <summary>
		/// The priority of a packet.
		/// Packets with a lower priority will be applied first.
		/// </summary>
		public virtual int Priority => 0;

		public string SearchableText => ToString();

		public string SearchableDetail => Id.Creator.ToString();

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
				packetType += subId;

				output.Write(packetType);
				output.Write(Timestamp.ToBinary());
			}
			output.Write(Data);
		}

		/// <summary>
		/// Applies the content of this packet to a lager.
		/// The packet has to deserialise itself from Data.
		/// </summary>
		public abstract Task Deserialise(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context);

		public override string ToString()
		{
			return string.Format("{0} Id {1} {2}({3})", Timestamp.ToString("yyyy-MM-dd HH:mm:ss"), Id, GetType().Name, contentString ?? "null");
		}

		/// <summary>
		/// A DataPacket is above another packet if the priority is lower or
		/// (if the priority is equal) if the Timestamp is older.
		/// </summary>
		/// <returns>The result of the comparison.</returns>
		/// <param name="other">The packet to compare to.</param>
		public virtual int CompareTo(DataPacket other)
		{
			if (Priority == other.Priority)
				return Timestamp.CompareTo(other.Timestamp);
			return Priority.CompareTo(other.Priority);
		}
	}
}
