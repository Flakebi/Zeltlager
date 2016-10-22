using System;
using System.Collections.Generic;
using System.IO;

namespace Zeltlager
{
	using Serialisation;

	/// <summary>
	/// Stores the status of a lager: How many packets were created by each collaborator.
	/// </summary>
	public class LagerStatus : ISerialisable<LagerSerialisationContext>
	{
		/// <summary>
		/// The collaborator list saves the packet count for each collaborator.
		/// The index in this list is the collaborator id as seen from the owner of this object.
		/// </summary>
		public List<Tuple<Collaborator, ushort>> PacketCount { get; private set; }

		public LagerStatus()
		{
			PacketCount = new List<Tuple<Collaborator, ushort>>();
		}

		public void Write(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			output.Write((byte)PacketCount.Count);
			foreach (var c in PacketCount)
			{
				// Write the collaborator id from our point of view
				serialiser.WriteId(output, context, c.Item1);
				serialiser.Write(output, context, c.Item2);
			}
		}

		public void WriteId(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			throw new InvalidOperationException("You can't write the id of a lager status");
		}

		public void Read(BinaryReader input, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			byte count = input.ReadByte();
			PacketCount.Capacity = count;
			for (int i = 0; i < count; i++)
			{
				Collaborator collaborator = serialiser.ReadFromId<Collaborator>(input, context);
				ushort packets = serialiser.Read(input, context, (ushort)0);
				PacketCount.Add(new Tuple<Collaborator, ushort>(collaborator, packets));
			}
		}
	}
}
