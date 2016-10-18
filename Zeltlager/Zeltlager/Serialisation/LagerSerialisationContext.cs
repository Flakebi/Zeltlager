namespace Zeltlager.Serialisation
{
	using Client;

	public class LagerSerialisationContext
	{
		public LagerBase Lager { get; private set; }
		public Collaborator Collaborator { get; private set; }

		public LagerSerialisationContext(LagerBase lager, Collaborator collaborator)
		{
			Lager = lager;
			Collaborator = collaborator;
		}
	}

	public class LagerClientSerialisationContext : LagerSerialisationContext
	{
		public LagerClient LagerClient
		{
			get
			{
				return (LagerClient)Lager;
			}
		}

		public LagerClientSerialisationContext(LagerClient lager, Collaborator collaborator) :
			base(lager, collaborator)
		{ }
	}
}
