using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PCLCrypto;

namespace Zeltlager
{
	public struct KeyPair
	{
		public KeyPair(byte[] publicKey, byte[] privateKey)
		{
			PublicKey = publicKey;
			PrivateKey = privateKey;
		}

		public byte[] PublicKey;
		public byte[] PrivateKey;
	}

	public class Crypto
	{
		public const int ASYMMETRIC_KEY_LENGTH = 512;
		public const int SYMMETRIC_KEY_LENGTH = 16;
		public const int SALT_LENGTH = 16;
		public const int IV_LENGTH = 16;

		const int ASYMMETRIC_KEY_SIZE = 4096;
		const int KEY_DERIVATION_ITERATIONS = 5000;
		static readonly IAsymmetricKeyAlgorithmProvider ASYMMETRIC_KEY_PROVIDER = WinRTCrypto.AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPkcs1Sha256);

		public static byte[] DeriveSymmetricKey(string password, byte[] salt)
		{
			return NetFxCrypto.DeriveBytes.GetBytes(password, salt, KEY_DERIVATION_ITERATIONS, SYMMETRIC_KEY_LENGTH);
		}

		public static byte[] GetRandom(int length)
		{
			byte[] cryptoRandomBuffer = new byte[length];
			NetFxCrypto.RandomNumberGenerator.GetBytes(cryptoRandomBuffer);
			return cryptoRandomBuffer;
		}

		public static byte[] ComputeMac(byte[] keyMaterial, byte[] data)
		{
			var algorithm = WinRTCrypto.MacAlgorithmProvider.OpenAlgorithm(MacAlgorithm.HmacSha512);
			CryptographicHash hasher = algorithm.CreateHash(keyMaterial);
			hasher.Append(data);
			return hasher.GetValueAndReset();
		}

		public static byte[] EncryptSymetric(byte[] keyMaterial, byte[] iv, byte[] data)
		{
			var provider = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
			var key = provider.CreateSymmetricKey(keyMaterial);

			return WinRTCrypto.CryptographicEngine.Encrypt(key, data, iv);
		}

		public static byte[] DecryptSymetric(byte[] keyMaterial, byte[] iv, byte[] data)
		{
			var provider = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
			var key = provider.CreateSymmetricKey(keyMaterial);

			return WinRTCrypto.CryptographicEngine.Decrypt(key, data, iv);
		}

		/// <summary>
		/// Creates a public and a private key.
		/// </summary>
		/// <returns>A tuple of the generated public and private key.</returns>
		public static KeyPair CreateAsymmetricKeys()
		{
			ICryptographicKey generatedKeyPair = ASYMMETRIC_KEY_PROVIDER.CreateKeyPair(ASYMMETRIC_KEY_SIZE);
			var publicKey = generatedKeyPair.ExportPublicKey(CryptographicPublicKeyBlobType.Pkcs1RsaPublicKey);
			var privateKey = generatedKeyPair.Export();
			if (publicKey.Length != ASYMMETRIC_KEY_LENGTH)
				throw new Exception("Wrong public key length");
			if (privateKey.Length != ASYMMETRIC_KEY_LENGTH)
				throw new Exception("Wrong private key length");
			return new KeyPair(publicKey, privateKey);
		}

		public static byte[] Sign(byte[] privateKey, byte[] data)
		{
			ICryptographicKey key = ASYMMETRIC_KEY_PROVIDER.ImportKeyPair(privateKey);
			return WinRTCrypto.CryptographicEngine.Sign(key, data);
		}

		public static bool Verify(byte[] publicKey, byte[] signature, byte[] data)
		{
			ICryptographicKey key = ASYMMETRIC_KEY_PROVIDER.ImportPublicKey(publicKey, CryptographicPublicKeyBlobType.Pkcs1RsaPublicKey);
			return WinRTCrypto.CryptographicEngine.VerifySignature(key, data, signature);
		}
	}
}
