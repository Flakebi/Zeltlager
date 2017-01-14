using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Cryptography;
	using Serialisation;

	/// <summary>
	/// Stores the status of a lager: How many packets were created by each collaborator.
	/// </summary>
	public class LagerStatus : ISerialisable<LagerSerialisationContext>
	{
		/// <summary>
		/// The collaborator list saves the bundle count for each collaborator.
		/// This is the next bundle id that can be uploaded to the server.
		/// The index in this list is the collaborator id as seen from the owner of this object.
		/// </summary>
		public List<Tuple<KeyPair, int>> BundleCount { get; private set; } = new List<Tuple<KeyPair, int>>();

		public async Task Write(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			output.Write(BundleCount.Count);
			foreach (var c in BundleCount)
			{
				// Write the collaborator order from the point of view of this object
				output.WritePublicKey(c.Item1);
				await serialiser.Write(output, context, c.Item2);
			}
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			throw new InvalidOperationException("You can't write the id of a lager status");
		}

		public async Task Read(BinaryReader input, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			int count = input.ReadByte();
			BundleCount = new List<Tuple<KeyPair, int>>(count);
			for (int i = 0; i < count; i++)
			{
				KeyPair key = input.ReadPublicKey();
				int packets = await serialiser.Read(input, context, 0);
				BundleCount.Add(new Tuple<KeyPair, int>(key, packets));
			}
		}
	}
}
