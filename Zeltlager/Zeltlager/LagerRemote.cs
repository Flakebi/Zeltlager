namespace Zeltlager
{
	using Serialisation;

	/// <summary>
	/// The remote part of a lager used for synchronisation.
	/// </summary>
	public class LagerRemote
	{
		/// <summary>
		/// The id of this lager on the server.
		/// </summary>
		[Serialisation]
		public int Id { get; private set; }

		/// <summary>
		/// The number of packets that were generated so far by each client.
		/// The collaborator order and packet count is the one of the server.
		/// </summary>
		[Serialisation]
		public LagerStatus Status { get; private set; }

		public LagerRemote()
		{
			Status = new LagerStatus();
		}

		public LagerRemote(int id) : this()
		{
			Id = id;
		}
	}
}
