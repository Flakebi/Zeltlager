namespace Zeltlager.Client
{
	public enum NetworkStatus
	{
		Connecting,
		ListLagers,
		RegisterCollaborator,
		LagerStatusRequest,
		CollaboratorDataRequest,
		BundlesRequest,
		DownloadBundles,
		UploadBundles,
		AddLager,
		Finished
	}

	public static class NetworkStatusHelper
	{
		public static string GetMessage(this NetworkStatus status)
		{
			switch (status)
			{
				case NetworkStatus.Connecting:
					return "Verbindung wird aufgebaut";
				case NetworkStatus.ListLagers:
					return "Lager werden gesammelt";
				case NetworkStatus.RegisterCollaborator:
					return "Handy wird registriert";
				case NetworkStatus.LagerStatusRequest:
					return "Lagerstatus wird ermittelt";
				case NetworkStatus.CollaboratorDataRequest:
					return "Daten über andere Handys werden gesammelt";
				case NetworkStatus.BundlesRequest:
					return "Neue Datenblöcke werden angefragt";
				case NetworkStatus.DownloadBundles:
					return "Datenblöcke werden heruntergeladen";
				case NetworkStatus.UploadBundles:
					return "Datenblöcke werden hochgeladen";
				case NetworkStatus.AddLager:
					return "Neues Lager wird hinzugefügt";
				case NetworkStatus.Finished:
					return "Fertig :D";
				default:
					return "Da hat einer einen Netzwerstatus nicht benannt...";
			}
		}
	}
}
