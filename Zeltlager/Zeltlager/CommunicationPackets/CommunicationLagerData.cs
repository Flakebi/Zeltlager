namespace Zeltlager.CommunicationPackets
{
	using Client;
	
	/// <summary>
	/// Data that is sent and received witha LagerCommunicationRequest.
	/// </summary>
	public class CommunicationLagerData
	{
		// General data
		public byte[] Unencrypted { get; }
		public byte[] Encrypted { get; }
		public LagerBase Lager { get; }

		// Send only data
		public LagerClient LagerClient => (LagerClient)Lager;


		// Receive only data
		public Collaborator Collaborator { get; }

		public CommunicationLagerData(LagerClient lager, byte[] unencrypted, byte[] encrypted)
		{
			Lager = lager;
			Unencrypted = unencrypted;
			Encrypted = encrypted;
		}

		public CommunicationLagerData(LagerBase lager, Collaborator collaborator, byte[] unencrypted, byte[] encrypted)
		{
			Lager = lager;
			Collaborator = collaborator;
			Unencrypted = unencrypted;
			Encrypted = encrypted;
		}
	}
}
