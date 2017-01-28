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
		Ready
	}
}
