using System.IO;
using System.Threading.Tasks;
using System;

namespace Zeltlager.CommunicationPackets.Requests
{
	using Client;
	using Network;
	using Serialisation;

    public class Register : CommunicationRequest
    {
		public static async Task<Register> Create(LagerClient lager)
		{
			var result = new Register();
			await result.Init(lager);
			return result;
		}

		Register() { }

		async Task Init(LagerClient lager)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(lager.Remote.Id);
				var context = new LagerSerialisationContext(lager.Manager, lager);
				await lager.Serialiser.Write(output, context, lager.OwnCollaborator);
			}
			Data = mem.ToArray();
		}

		public override async Task Apply(INetworkConnection connection, LagerManager manager)
		{
			bool success = false;
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				int lagerId = input.ReadInt32();
				// Check if the lager exists
				if (manager.Lagers.ContainsKey(lagerId))
				{
					LagerBase lager = manager.Lagers[lagerId];

					var context = new LagerSerialisationContext(manager, lager);
					Collaborator collaborator = new Collaborator();
					try
					{
						await lager.Serialiser.Read(input, context, collaborator);

						// Check if the collaborator already registered
						if (!lager.Collaborators.ContainsKey(collaborator.Key))
							// Add the collaborator
							await lager.AddCollaborator(collaborator);
						else
							collaborator = lager.Collaborators[collaborator.Key];
						await connection.WritePacket(new Responses.Register(lager.Status.GetCollaboratorId(collaborator)));
						success = true;
					}
					catch (Exception e)
					{
						await LagerManager.Log.Exception("Register collaborator", e);
					}
				}
			}

			// Create a response
			if (!success)
				await connection.WritePacket(new Responses.Status(success));
		}
	}
}
