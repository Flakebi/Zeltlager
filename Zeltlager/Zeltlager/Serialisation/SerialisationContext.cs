namespace Zeltlager.Serialisation
{
	using Client;

	public class SerialisationContext
	{
		public LagerClient Lager { get; set; }
		public Collaborator Collaborator { get; set; }

		public SerialisationContext(LagerClient lager, Collaborator collaborator)
		{
			Lager = lager;
			Collaborator = collaborator;
		}
	}
}
