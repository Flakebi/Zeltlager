using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.Cryptography
{
	/// <summary>
	/// Constants used for the cryptographic functions.
	/// All LENGTH values are an amount of bytes,
	/// all SIZE values are an amount of bits.
	/// </summary>
	public static class CryptoConstants
	{
		public const int SYMMETRIC_KEY_LENGTH = 32;
		public const int SYMMETRIC_BLOCK_LENGTH = 16;
		public const int SALT_LENGTH = 16;
		public const int IV_LENGTH = 16;
		public const int HASH_LENGTH = 32;
		public const int MAC_LENGTH = HASH_LENGTH;
		public const int SIGNATURE_LENGTH = 128;

		public const int ASYMMETRIC_KEY_SIZE = 1024;
		public const int KEY_DERIVATION_ITERATIONS = 5000;

		public static readonly byte[] DEFAULT_PUBLIC_KEY = { 1, 0, 1 };
	}

	public struct KeyPair : IEquatable<KeyPair>
	{
		public readonly byte[] Modulus;
		public readonly byte[] PublicKey;
		public byte[] PrivateKey;

		public KeyPair(byte[] modulus, byte[] publicKey, byte[] privateKey)
		{
			Modulus = modulus;
			PublicKey = publicKey;
			PrivateKey = privateKey;
		}

		public bool Equals(KeyPair other)
		{
			return Modulus.SequenceEqual(other.Modulus);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is KeyPair))
				return false;
			return Equals((KeyPair)obj);
		}

		public override int GetHashCode()
		{
			return Modulus.GetHashCode();
		}

		public static bool operator ==(KeyPair p1, KeyPair p2)
		{
			return p1.Equals(p2);
		}

		public static bool operator !=(KeyPair p1, KeyPair p2)
		{
			return !(p1 == p2);
		}
	}

	public static class KeyPairHelper
	{
		public static void WritePublicKey(this BinaryWriter output, KeyPair keys)
		{
			output.Write((ushort)keys.Modulus.Length);
			output.Write(keys.Modulus);
		}

		public static void WritePrivateKey(this BinaryWriter output, KeyPair keys)
		{
			output.WritePublicKey(keys);
			output.Write((ushort)keys.PrivateKey.Length);
			output.Write(keys.PrivateKey);
		}

		public static KeyPair ReadPublicKey(this BinaryReader input)
		{
			ushort length = input.ReadUInt16();
			return new KeyPair(input.ReadBytes(length), CryptoConstants.DEFAULT_PUBLIC_KEY, null);
		}

		public static KeyPair ReadPrivateKey(this BinaryReader input)
		{
			var key = input.ReadPublicKey();
			ushort length = input.ReadUInt16();
			key.PrivateKey = input.ReadBytes(length);
			return key;
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
		Task<KeyPair> CreateAsymmetricKey();

		/// <summary>
		/// Sign the given data.
		/// </summary>
		/// <param name="key">The key that should be used for signing.</param>
		/// <param name="data">The data that should be signed.</param>
		/// <returns>The created signature.</returns>
		Task<byte[]> Sign(KeyPair key, byte[] data);

		/// <summary>
		/// Verify the signature of some data.
		/// </summary>
		/// <param name="key">The used key.</param>
		/// <param name="signature">The given signature.</param>
		/// <param name="data">The data that should be verified.</param>
		/// <returns>If the data could be verified successfully or the signature is incorrect.</returns>
		Task<bool> Verify(KeyPair key, byte[] signature, byte[] data);
	}
}
