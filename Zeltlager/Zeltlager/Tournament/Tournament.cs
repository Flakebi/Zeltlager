namespace Zeltlager.Tournament
{
	using Client;

    public class Tournament : ILagerPart
    {
		LagerClient lager;

        public Tournament(LagerClient lager)
        {
            this.lager = lager;
        }
    }
}
