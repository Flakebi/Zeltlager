using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Zeltlager.Cryptography
{
	using static CryptoConstants;
	
	/// <summary>
	/// A crypto provider using BouncyCastle.
	/// </summary>
	public class BCCryptoProvider : ICryptoProvider
	{
		SecureRandom random = new SecureRandom();

		PaddedBufferedBlockCipher GetSymmetricCipher()
		{
			return new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesFastEngine()), new Pkcs7Padding());
		}

		RsaEngine GetAsymmetricCipher() => new RsaEngine();

		public Task<byte[]> GetRandom(int length)
		{
			return Task.Run(() =>
			{
				byte[] result = new byte[length];
				random.NextBytes(result);
				return result;
			});
		}

		public Task<byte[]> Hash(byte[] data)
		{
			return Task.Run(() =>
			{
				var digest = new Sha256Digest();
				digest.BlockUpdate(data, 0, data.Length);
				var result = new byte[HASH_LENGTH];
				digest.DoFinal(result, 0);
				return result;
			});
		}

		public Task<byte[]> ComputeMac(byte[] key, byte[] data)
		{
			return Task.Run(() =>
			{
				var mac = new HMac(new Sha256Digest());
				mac.Init(new KeyParameter(key));
				mac.BlockUpdate(data, 0, data.Length);
				var result = new byte[MAC_LENGTH];
				mac.DoFinal(result, 0);
				return result;
			});
		}

		public Task<byte[]> DeriveSymmetricKey(string password, byte[] salt)
		{
			return Task.Run(() => SCrypt.Generate(Encoding.UTF8.GetBytes(password), salt, 16384, 8, 1, SYMMETRIC_KEY_LENGTH));
		}

		public Task<int> GetSymmetricEncryptedLength(int dataLength)
		{
			// At least 1 byte padding so we need a whole block if the dataLength fits into blocks
			return Task.Run(() =>
			{
				int rest = dataLength % SYMMETRIC_BLOCK_LENGTH;

				if (rest == 0)
					return dataLength + SYMMETRIC_BLOCK_LENGTH;
				return dataLength - rest + SYMMETRIC_BLOCK_LENGTH;
			});
		}

		public Task<byte[]> EncryptSymetric(byte[] key, byte[] iv, byte[] data)
		{
			return Task.Run(() =>
			{
				var symmetricCipher = GetSymmetricCipher();
				symmetricCipher.Init(true, new ParametersWithIV(new KeyParameter(key), iv));
				byte[] result = new byte[symmetricCipher.GetOutputSize(data.Length)];
				symmetricCipher.DoFinal(data, result, 0);
				return result;
			});
		}

		public Task<byte[]> DecryptSymetric(byte[] key, byte[] iv, byte[] data)
		{
			return Task.Run(() =>
			{
				var symmetricCipher = GetSymmetricCipher();
				symmetricCipher.Init(false, new ParametersWithIV(new KeyParameter(key), iv));
				byte[] result = new byte[symmetricCipher.GetOutputSize(data.Length)];
				int length = symmetricCipher.DoFinal(data, result, 0);
				result = result.Take(length).ToArray();
				return result;
			});
		}

		public Task<KeyPair> CreateAsymmetricKey()
		{
			return Task.Run(() =>
			{
				var generator = new RsaKeyPairGenerator();
				generator.Init(new KeyGenerationParameters(random, ASYMMETRIC_KEY_SIZE));
				var keyPair = generator.GenerateKeyPair();
				var publicKey = (RsaKeyParameters)keyPair.Public;
				var privateKey = (RsaPrivateCrtKeyParameters)keyPair.Private;

				var modulus = publicKey.Modulus.ToByteArray();
				var pub = publicKey.Exponent.ToByteArray();
				var priv = privateKey.Exponent.ToByteArray();
				return new KeyPair(modulus, pub, priv);
			});
		}

		public Task<byte[]> Sign(KeyPair key, byte[] data)
		{
			return Task.Run(async () =>
			{
				// Hash data
				var hash = await Hash(data);
				// Encrypt/Sign hash
				var asymmetricCipher = GetAsymmetricCipher();
				asymmetricCipher.Init(true, new RsaKeyParameters(true, new BigInteger(key.Modulus), new BigInteger(key.PrivateKey)));
				return asymmetricCipher.ProcessBlock(hash, 0, hash.Length);
			});
		}

		public Task<bool> Verify(KeyPair key, byte[] signature, byte[] data)
		{
			return Task.Run(async () =>
			{
				// Hash data
				var hash = await Hash(data);
				// Decrypt/Verify hash
				var asymmetricCipher = GetAsymmetricCipher();
				asymmetricCipher.Init(false, new RsaKeyParameters(true, new BigInteger(key.Modulus), new BigInteger(key.PublicKey)));
				var result = asymmetricCipher.ProcessBlock(signature, 0, signature.Length);
				return Enumerable.SequenceEqual(hash, result);
			});
		}
	}
}
