using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager
{
	public static class CryptoConstants
	{
		public const int SYMMETRIC_KEY_LENGTH = 32;
		public const int SYMMETRIC_BLOCK_LENGTH = 16;
		public const int SALT_LENGTH = 16;
		public const int IV_LENGTH = 16;
		public const int HASH_LENGTH = 32;
		public const int MAC_LENGTH = HASH_LENGTH;
		public const int MODULUS_LENGTH = 513;
		public const int PRIVATE_KEY_LENGTH = 512;

		public const int ASYMMETRIC_KEY_SIZE = 4096;
		public const int KEY_DERIVATION_ITERATIONS = 5000;
	}

	public struct KeyPair
	{
		public byte[] Modulus;
		public byte[] PublicKey;
		public byte[] PrivateKey;

		public KeyPair(byte[] modulus, byte[] publicKey, byte[] privateKey)
		{
			Modulus = modulus;
			PublicKey = publicKey;
			PrivateKey = privateKey;
		}
	}

	public interface ICryptoProvider
	{
		byte[] GetRandom(int length);
		byte[] Hash(byte[] data);
		byte[] ComputeMac(byte[] keyMaterial, byte[] data);
		byte[] DeriveSymmetricKey(string password, byte[] salt);

		byte[] EncryptSymetric(byte[] keyMaterial, byte[] iv, byte[] data);
		byte[] DecryptSymetric(byte[] keyMaterial, byte[] iv, byte[] data);

		/// <summary>
		/// Creates a public and a private key.
		/// </summary>
		/// <returns>A tuple of the generated public and private key.</returns>
		KeyPair CreateAsymmetricKeys();

		byte[] Sign(byte[] modulus, byte[] privateKey, byte[] data);
		bool Verify(byte[] modulus, byte[] publicKey, byte[] signature, byte[] data);
		/// <summary>
		/// Verify using the default public key {1, 0, 1} (65537)
		/// </summary>
		/// <param name="modulus"></param>
		/// <param name="signature"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		bool Verify(byte[] modulus, byte[] signature, byte[] data);
	}
}
