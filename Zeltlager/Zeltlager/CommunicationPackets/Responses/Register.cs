namespace Zeltlager.CommunicationPackets.Responses
{
	public class Register : CommunicationResponse
	{
		Register() { }
		
		public Register(int collaboratorId)
		{
			Data = collaboratorId.ToBytes();
		}

		/// <summary>
		/// The collaborator id that the client got from the server.
		/// </summary>
		/// <returns>The obtained collaborator id.</returns>
		public int GetCollaboratorId()
		{
			return Data.ToInt(0);
		}
	}
}
