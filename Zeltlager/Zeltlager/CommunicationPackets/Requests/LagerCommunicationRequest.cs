using System;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.CommunicationPackets.Requests
{
	using Client;
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
		/// <param name="lager">
		/// The lager that is refered by this packet.
		/// </param>
		/// <param name="unencrypted">
		/// The unencrypted data that should be sent with the packet.
		/// </param>
		/// <param name="encrypted">
		/// The encrypted data that should be sent with the packet.
		/// </param>
		protected async Task CreateData(LagerClient lager, byte[] unencrypted, byte[] encrypted)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(lager.Remote.Id);
				output.Write(lager.Remote.Status.BundleCount.FindIndex(
					c => c.Item1 == lager.OwnCollaborator.Key));
				// Write unencrypted data
				output.Write(unencrypted.Length);
				output.Write(unencrypted);
				// Encrypt data
				byte[] iv = await LagerManager.CryptoProvider.GetRandom(CryptoConstants.IV_LENGTH);
				encrypted = await LagerManager.CryptoProvider.EncryptSymetric(
					lager.Data.SymmetricKey, iv, encrypted);
				output.Write(encrypted.Length);
				output.Write(encrypted);
				// Sign data
				byte[] data = mem.ToArray();
				output.Write(await LagerManager.CryptoProvider.Sign(
					lager.Data.AsymmetricKey, data));
				data = mem.ToArray();
				output.Write(await LagerManager.CryptoProvider.Sign(
					lager.OwnCollaborator.Key, data));
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
		protected async Task<Tuple<Collaborator, byte[], byte[]>> GetData(LagerManager manager)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				// Get the lager
				int lagerId = input.ReadInt32();
				if (!manager.Lagers.ContainsKey(lagerId))
					throw new LagerException("Lager not found");
				LagerBase lager = manager.Lagers[input.ReadInt32()];
				// Get the collaborator
				int collaboratorId = input.ReadInt32();
				if (collaboratorId < 0 || collaboratorId >= lager.Status.BundleCount.Count)
					throw new LagerException("Collaborator not found");
				KeyPair collaboratorKey = lager.Status.BundleCount[collaboratorId].Item1;
				Collaborator collaborator = lager.Collaborators[collaboratorKey];
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
				return new Tuple<Collaborator, byte[], byte[]>(collaborator, unencrypted, encrypted);
			}
		}
	}
}