using System;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.CommunicationPackets.Requests
{
	using Cryptography;
	
	public abstract class LagerCommunicationRequest : CommunicationRequest
	{
		/// <summary>
		/// Write the id of the lager,
		/// our collaborator id,
		/// the unencrypted data,
		/// the encrypted data (that will be encrypted with the synchronous
		/// lager key in this function) and
		/// sign everything with the collaborator key.
		/// </summary>
		/// <param name="data">
		/// The data to create the packet
		/// </param>
		protected async Task CreateData(CommunicationLagerData data)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(data.Lager.Remote.Id);
				output.Write(data.Lager.Remote.Status.GetCollaboratorId(data.LagerClient.OwnCollaborator));
				// Timestamp
				output.Write(DateTime.UtcNow.ToBinary());
				// Write unencrypted data
				output.Write(data.Unencrypted.Length);
				output.Write(data.Unencrypted);
				// Encrypt data
				byte[] iv = await LagerManager.CryptoProvider.GetRandom(CryptoConstants.IV_LENGTH);
				byte[] encrypted = await LagerManager.CryptoProvider.EncryptSymetric(
					data.Lager.Data.SymmetricKey, iv, data.Encrypted);
				output.Write(encrypted.Length);
				output.Write(encrypted);
				// Sign data
				byte[] bytes = mem.ToArray();
				output.Write(await LagerManager.CryptoProvider.Sign(
					data.Lager.Data.AsymmetricKey, bytes));
				bytes = mem.ToArray();
				output.Write(await LagerManager.CryptoProvider.Sign(
					data.LagerClient.OwnCollaborator.Key, bytes));
			}
			Data = mem.ToArray();
		}

		/// <summary>
		/// Get the unencrypted and encrypted data from the packet data.
		/// The signarures will be verified and the data will be unencrypted.
		/// </summary>
		/// <returns>
		/// The collaborator that sent the packet,
		/// the unencrypted data and
		/// the encrypted data.
		/// This function throws an exception if an error occurs.
		/// </returns>
		protected async Task<CommunicationLagerData> GetData(LagerManager manager)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				// Get the lager
				int lagerId = input.ReadInt32();
				if (!manager.Lagers.ContainsKey(lagerId))
					throw new LagerException("Lager not found");
				LagerBase lager = manager.Lagers[lagerId];
				// Get the collaborator
				int collaboratorId = input.ReadInt32();
				if (collaboratorId < 0 || collaboratorId >= lager.Status.BundleCount.Count)
					throw new LagerException("Collaborator not found");
				KeyPair collaboratorKey = lager.Status.BundleCount[collaboratorId].Item1;
				Collaborator collaborator = lager.Collaborators[collaboratorKey];
				// Timestamp
				var timestamp = DateTime.FromBinary(input.ReadInt64());
				var diff = DateTime.UtcNow - timestamp;
				if (diff.TotalMinutes < 0 || diff.TotalMinutes > 1)
					throw new LagerException("Message too old");
				// Data
				int length = input.ReadInt32();
				byte[] unencrypted = input.ReadBytes(length);
				length = input.ReadInt32();
				byte[] encrypted = input.ReadBytes(length);

				// Check the signature
				byte[] signature = new byte[CryptoConstants.SIGNATURE_LENGTH];
				Array.Copy(Data, Data.Length - CryptoConstants.SIGNATURE_LENGTH,
					signature, 0, signature.Length);
				byte[] data = new byte[Data.Length - CryptoConstants.SIGNATURE_LENGTH];
				Array.Copy(Data, data, data.Length);
				if (!await LagerManager.CryptoProvider.Verify(collaborator.Key, signature, data))
					throw new LagerException("Invalid collaborator signature");
				return new CommunicationLagerData(lager, collaborator, unencrypted, encrypted);
			}
		}
	}
}