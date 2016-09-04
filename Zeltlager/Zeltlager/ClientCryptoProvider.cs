using System;
using PCLCrypto;

using static Zeltlager.CryptoConstants;

namespace Zeltlager
{
	public class ClientCryptoProvider : ICryptoProvider
	{
		const int ASYMMETRIC_KEY_SIZE = 4096;
		const int KEY_DERIVATION_ITERATIONS = 5000;
		static readonly IAsymmetricKeyAlgorithmProvider ASYMMETRIC_KEY_PROVIDER = WinRTCrypto.AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPkcs1Sha256);
		static readonly ISymmetricKeyAlgorithmProvider SYMMETRIC_KEY_PROVIDER = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);

		public byte[] Hash(byte[] data)
		{
			return data;
		}

		public byte[] DeriveSymmetricKey(string password, byte[] salt)
		{
			return NetFxCrypto.DeriveBytes.GetBytes(password, salt, KEY_DERIVATION_ITERATIONS, SYMMETRIC_KEY_LENGTH);
		}

		public byte[] GetRandom(int length)
		{
			byte[] cryptoRandomBuffer = new byte[length];
			NetFxCrypto.RandomNumberGenerator.GetBytes(cryptoRandomBuffer);
			return cryptoRandomBuffer;
		}

		public byte[] ComputeMac(byte[] keyMaterial, byte[] data)
		{
			var algorithm = WinRTCrypto.MacAlgorithmProvider.OpenAlgorithm(MacAlgorithm.HmacSha512);
			CryptographicHash hasher = algorithm.CreateHash(keyMaterial);
			hasher.Append(data);
			byte[] hmac = hasher.GetValueAndReset();
			if (hmac.Length != MAC_LENGTH)
				throw new Exception("Wrong HMac length");
			return hmac;
		}

		public byte[] EncryptSymetric(byte[] keyMaterial, byte[] iv, byte[] data)
		{
			var key = SYMMETRIC_KEY_PROVIDER.CreateSymmetricKey(keyMaterial);

			return WinRTCrypto.CryptographicEngine.Encrypt(key, data, iv);
		}

		public byte[] DecryptSymetric(byte[] keyMaterial, byte[] iv, byte[] data)
		{
			var key = SYMMETRIC_KEY_PROVIDER.CreateSymmetricKey(keyMaterial);

			return WinRTCrypto.CryptographicEngine.Decrypt(key, data, iv);
		}

		/// <summary>
		/// Creates a public and a private key.
		/// </summary>
		/// <returns>A tuple of the generated public and private key.</returns>
		public KeyPair CreateAsymmetricKeys()
		{
			ICryptographicKey generatedKeyPair = ASYMMETRIC_KEY_PROVIDER.CreateKeyPair(ASYMMETRIC_KEY_SIZE);
			var parameters = generatedKeyPair.ExportParameters(true);
			return new KeyPair(parameters.Modulus, parameters.DP, parameters.D);
		}

		public byte[] Sign(byte[] modulus, byte[] privateKey, byte[] data)
		{
			var parameters = new RSAParameters();
			parameters.Modulus = modulus;
			parameters.D = privateKey;
			ICryptographicKey key = ASYMMETRIC_KEY_PROVIDER.ImportParameters(parameters);
			return WinRTCrypto.CryptographicEngine.Sign(key, data);
		}

		public bool Verify(byte[] modulus, byte[] publicKey, byte[] signature, byte[] data)
		{
			var parameters = new RSAParameters();
			parameters.Modulus = modulus;
			parameters.DP = publicKey;
			ICryptographicKey key = ASYMMETRIC_KEY_PROVIDER.ImportParameters(parameters);
			return WinRTCrypto.CryptographicEngine.VerifySignature(key, data, signature);
		}

		public bool Verify(byte[] modulus, byte[] signature, byte[] data)
		{
			return Verify(modulus, new byte[] { 1, 0, 1 }, signature, data);
		}
	}
}
