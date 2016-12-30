using System;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Cryptography;

	public class LagerData
	{
		/// <summary>
		/// The data of this lager.
		/// This contains the version, the public key, salt, iv and (encrypted) the name and private key.
		/// All this data is signed with the lager private key.
		/// </summary>
		public byte[] Data { get; private set; }

		// Semantic data
		public byte[] Salt { get; private set; }
		public byte[] SymmetricKey { get; private set; }
		public KeyPair AsymmetricKey { get; private set; }
		public string Name { get; private set; }

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
					throw new InvalidDataException("The lager has an unknown version");
				AsymmetricKey = reader.ReadPublicKey();
			}

			// Check if the data signature is valid
			byte[] signature = new byte[CryptoConstants.SIGNATURE_LENGTH];
			byte[] signedData = new byte[Data.Length - signature.Length];
			Array.Copy(Data, Data.Length - signature.Length, signature, 0, signature.Length);
			Array.Copy(Data, signedData, signedData.Length);
			if (!await LagerManager.CryptoProvider.Verify(AsymmetricKey, signature, signedData))
				throw new InvalidDataException("The signature of the lager is wrong");
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
