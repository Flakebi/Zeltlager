using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using static Zeltlager.CryptoConstants;

namespace Zeltlager.Server
{
	public class ServerCryptoProvider : ICryptoProvider
	{
		static readonly Aes SYMMETRIC_ALGORITHM = Aes.Create();

		public byte[] Hash(byte[] data)
		{
			return data;
		}

		public byte[] DeriveSymmetricKey(string password, byte[] salt)
		{
			return new Rfc2898DeriveBytes(password, salt, KEY_DERIVATION_ITERATIONS).GetBytes(SYMMETRIC_KEY_LENGTH);
		}

		public byte[] GetRandom(int length)
		{
			byte[] cryptoRandomBuffer = new byte[length];
			RandomNumberGenerator.Create().GetBytes(cryptoRandomBuffer);
			return cryptoRandomBuffer;
		}

		public byte[] ComputeMac(byte[] keyMaterial, byte[] data)
		{
			var algorithm = HMAC.Create("HmacSha512");
			algorithm.Key = keyMaterial;
			byte[] hmac = algorithm.ComputeHash(data);
			if (hmac.Length != MAC_LENGTH)
				throw new Exception("Wrong HMac length");
			return hmac;
		}

		public byte[] EncryptSymetric(byte[] keyMaterial, byte[] iv, byte[] data)
		{
			var encryptor = SYMMETRIC_ALGORITHM.CreateEncryptor(keyMaterial, iv);
			// Full blocks or full blocks - 1 if there are no bytes after the last full block
			int fullBlocks = (data.Length - 1) / encryptor.InputBlockSize;
			byte[] result = new byte[fullBlocks * encryptor.OutputBlockSize];
			for (int i = 0; i < fullBlocks; i++)
				encryptor.TransformBlock(data, encryptor.InputBlockSize * i, encryptor.InputBlockSize, result, encryptor.OutputBlockSize * i);
			byte[] rest = encryptor.TransformFinalBlock(data, encryptor.InputBlockSize * fullBlocks, data.Length - encryptor.InputBlockSize * fullBlocks);
			byte[] all = result.Concat(rest).ToArray();

			return all;
		}

		public byte[] DecryptSymetric(byte[] keyMaterial, byte[] iv, byte[] data)
		{
			var decryptor = SYMMETRIC_ALGORITHM.CreateDecryptor(keyMaterial, iv);
			// Full blocks or full blocks - 1 if there are no bytes after the last full block
			int fullBlocks = (data.Length - 1) / decryptor.InputBlockSize;
			byte[] result = new byte[fullBlocks * decryptor.OutputBlockSize];
			for (int i = 0; i < fullBlocks; i++)
				decryptor.TransformBlock(data, decryptor.InputBlockSize * i, decryptor.InputBlockSize, result, decryptor.OutputBlockSize * i);
			byte[] rest = decryptor.TransformFinalBlock(data, decryptor.InputBlockSize * fullBlocks, data.Length - decryptor.InputBlockSize * fullBlocks);
			byte[] all = result.Concat(rest).ToArray();

			return all;
		}

		/// <summary>
		/// Creates a public and a private key.
		/// </summary>
		/// <returns>A tuple of the generated public and private key.</returns>
		public KeyPair CreateAsymmetricKeys()
		{
			var rsa = new RSACryptoServiceProvider(ASYMMETRIC_KEY_SIZE);
			var parameters = rsa.ExportParameters(true);
			return new KeyPair(parameters.Modulus, parameters.DP, parameters.D);
		}

		public byte[] Sign(byte[] modulus, byte[] privateKey, byte[] data)
		{
			var parameters = new RSAParameters();
			parameters.Modulus = modulus;
			parameters.D = privateKey;
			// Set other properties to random values
			parameters.DP = privateKey;
			parameters.DQ = privateKey;
			parameters.InverseQ = privateKey;
			parameters.P = privateKey;
			parameters.Q = privateKey;
			var rsa = new RSACryptoServiceProvider(ASYMMETRIC_KEY_SIZE);
			rsa.ImportParameters(parameters);
			return rsa.SignData(data, SHA256.Create());
		}

		public bool Verify(byte[] modulus, byte[] publicKey, byte[] signature, byte[] data)
		{
			var parameters = new RSAParameters();
			parameters.Modulus = modulus;
			parameters.DP = publicKey;
			var rsa = new RSACryptoServiceProvider(ASYMMETRIC_KEY_SIZE);
			rsa.ImportParameters(parameters);
			return rsa.VerifyData(data, SHA256.Create(), signature);
		}

		public bool Verify(byte[] modulus, byte[] signature, byte[] data)
		{
			return Verify(modulus, new byte[] { 1, 0, 1 }, signature, data);
		}
	}
}
