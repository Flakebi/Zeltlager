using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Serialisation;

	/// <summary>
	/// Stores the status of a lager: How many packets were created by each collaborator.
	/// </summary>
	public class LagerStatus : ISerialisable<LagerSerialisationContext>
	{
		/// <summary>
		/// The collaborator list saves the bundle count for each collaborator.
		/// The index in this list is the collaborator id as seen from the owner of this object.
		/// </summary>
		public List<Tuple<Collaborator, uint>> BundleCount { get; private set; }

		public LagerStatus()
		{
			BundleCount = new List<Tuple<Collaborator, uint>>();
		}

		public async Task Write(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			output.Write((byte)BundleCount.Count);
			foreach (var c in BundleCount)
			{
				// Write the collaborator id from our point of view
				await serialiser.WriteId(output, context, c.Item1);
				await serialiser.Write(output, context, c.Item2);
			}
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			throw new InvalidOperationException("You can't write the id of a lager status");
		}

		public async Task Read(BinaryReader input, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			byte count = input.ReadByte();
			BundleCount.Capacity = count;
			for (int i = 0; i < count; i++)
			{
				Collaborator collaborator = await serialiser.ReadFromId<Collaborator>(input, context);
				uint packets = await serialiser.Read(input, context, (uint)0);
				BundleCount.Add(new Tuple<Collaborator, uint>(collaborator, packets));
			}
		}
	}
}
