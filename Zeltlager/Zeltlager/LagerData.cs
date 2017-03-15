using System;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Cryptography;

	public class LagerData : ISearchable
	{
		/// <summary>
		/// The data of this lager.
		/// This contains the version, the public key, salt, iv and (encrypted) the name and private key.
		/// All this data is signed with the lager private key.
		/// </summary>
		public byte[] Data { get; private set; }

		// Semantic data
		/// <summary>
		/// The salt used for the key derivation functions.
		/// </summary>
		public byte[] Salt { get; set; }
		public byte[] SymmetricKey { get; set; }
		/// <summary>
		/// The asymmetric keys of this lager, the private key is null for the server.
		/// </summary>
		public KeyPair AsymmetricKey { get; set; }
		public string Name { get; set; }

		public string SearchableText => Name;

		public string SearchableDetail => "";

		public LagerData() { }
		
		public LagerData(byte[] data)
		{
			Data = data;
		}

		public async Task Verify()
		{
			using (BinaryReader reader = new BinaryReader(new MemoryStream(Data)))
			{
				int version = reader.ReadInt32();
				if (version != LagerBase.VERSION)
					throw new LagerException("The lager has an unknown version");
				AsymmetricKey = reader.ReadPublicKey();
			}

			// Check if the data signature is valid
			byte[] signature = new byte[CryptoConstants.SIGNATURE_LENGTH];
			byte[] signedData = new byte[Data.Length - signature.Length];
			Array.Copy(Data, Data.Length - signature.Length, signature, 0, signature.Length);
			Array.Copy(Data, signedData, signedData.Length);
			if (!await LagerManager.CryptoProvider.Verify(AsymmetricKey, signature, signedData))
				throw new LagerException("The signature of the lager is wrong");
		}

		/// <summary>
		/// Fill the data array.
		/// </summary>
		public async Task Serialise()
		{
			if (Data == null)
			{
				// Serialise the encrypted data
				MemoryStream mem = new MemoryStream();
				using (BinaryWriter writer = new BinaryWriter(mem))
				{
					writer.Write(Name);
					writer.WritePrivateKey(AsymmetricKey);
				}
				byte[] iv = await LagerManager.CryptoProvider.GetRandom(CryptoConstants.IV_LENGTH);
				byte[] encryptedData = await LagerManager.CryptoProvider.EncryptSymetric(SymmetricKey, iv, mem.ToArray());

				// Serialise the unencrypted data
				mem = new MemoryStream();
				using (BinaryWriter writer = new BinaryWriter(mem))
				{
					writer.Write(LagerBase.VERSION);
					writer.WritePublicKey(AsymmetricKey);
					writer.Write(Salt);
					writer.Write(iv);
					writer.Write(encryptedData);
					writer.Flush();

					// Sign the data
					byte[] signature = await LagerManager.CryptoProvider.Sign(AsymmetricKey, mem.ToArray());
					writer.Write(signature);
				}
				Data = mem.ToArray();
			}
		}

		/// <summary>
		/// Try to decrypt the data with the given symmetric key.
		/// </summary>
		/// <param name="password">The password to decrypt the data.</param>
		/// <returns>
		/// If the decryption was successful and the attributes contain useful data.
		/// </returns>
		public async Task<bool> Decrypt(string password)
		{
			try
			{
				// Read the encrypted data
				byte[] iv;
				byte[] encryptedData;
				using (BinaryReader input = new BinaryReader(new MemoryStream(Data)))
				{
					// Version
					input.ReadInt32();
					input.ReadPublicKey();
					Salt = input.ReadBytes(CryptoConstants.SALT_LENGTH);
					iv = input.ReadBytes(CryptoConstants.IV_LENGTH);
					encryptedData = input.ReadBytes((int)(input.BaseStream.Length
						- input.BaseStream.Position - CryptoConstants.SIGNATURE_LENGTH));
				}

				// Decrypt the data
				SymmetricKey = await LagerManager.CryptoProvider.DeriveSymmetricKey(password, Salt);
				byte[] unencryptedData = await LagerManager.CryptoProvider.DecryptSymetric(SymmetricKey, iv, encryptedData);
				using (BinaryReader input = new BinaryReader(new MemoryStream(unencryptedData)))
				{
					Name = input.ReadString();
					AsymmetricKey = input.ReadPrivateKey();
				}
			}
			catch
			{
				return false;
			}
			return true;
		}
	}

	public static class LagerDataHelper
	{
		public static void Write(this BinaryWriter output, LagerData data)
		{
			output.Write(data.Data.Length);
			output.Write(data.Data);
		}

		public static LagerData ReadLagerData(this BinaryReader input)
		{
			int len = input.ReadInt32();
			return new LagerData(input.ReadBytes(len));
		}
	}
}
