using System;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Serialisation;

	/// <summary>
	/// The remote part of a lager used for synchronisation.
	/// </summary>
	public class LagerRemote : ISerialisable<LagerSerialisationContext>
	{
		/// <summary>
		/// The id of this lager on the server.
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// The number of packets that were generated so far by each client.
		/// The collaborator order and packet count is the one of the server.
		/// </summary>
		public LagerStatus Status { get; private set; }

		public LagerRemote() { }

		public LagerRemote(int id)
		{
			Id = id;
			Status = new LagerStatus();
		}

		public async Task Write(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			output.Write(Id);
			await serialiser.Write(output, context, Status);
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			throw new InvalidOperationException("You can't write the id of a lager remote");
		}

		public async Task Read(BinaryReader input, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			Id = input.ReadInt32();
			Status = new LagerStatus();
			await serialiser.Read(input, context, Status);
		}
	}
}
