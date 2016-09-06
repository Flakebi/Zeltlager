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
		public const int SIGNATURE_LENGTH = 256;

		public const int ASYMMETRIC_KEY_SIZE = 2048;
		public const int KEY_DERIVATION_ITERATIONS = 5000;

		public static readonly byte[] DEFAULT_PUBLIC_KEY = new byte[] { 1, 0, 1 };
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
		Task<byte[]> GetRandom(int length);
		Task<byte[]> Hash(byte[] data);
		Task<byte[]> ComputeMac(byte[] key, byte[] data);
		Task<byte[]> DeriveSymmetricKey(string password, byte[] salt);

		Task<int> GetSymmetricEncryptedLength(int dataLength);
		Task<byte[]> EncryptSymetric(byte[] key, byte[] iv, byte[] data);
		Task<byte[]> DecryptSymetric(byte[] key, byte[] iv, byte[] data);

		/// <summary>
		/// Creates a public and a private key.
		/// </summary>
		/// <returns>A tuple of the generated public and private key.</returns>
		Task<KeyPair> CreateAsymmetricKeys();

		Task<byte[]> Sign(byte[] modulus, byte[] privateKey, byte[] data);
		Task<bool> Verify(byte[] modulus, byte[] publicKey, byte[] signature, byte[] data);
		/// <summary>
		/// Verify using the default public key (65537).
		/// </summary>
		/// <param name="modulus">The modulus parameter of the key.</param>
		/// <param name="signature">The given signature.</param>
		/// <param name="data">The data that should be verified.</param>
		/// <returns>If the data could be verified successfully or the signature is incorrect.</returns>
		Task<bool> Verify(byte[] modulus, byte[] signature, byte[] data);
	}
}
