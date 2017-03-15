using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using Zeltlager.Cryptography;
using System.Linq;

namespace UnitTests
{
	[TestFixture]
	public class CryptoTests
	{
		readonly ICryptoProvider crypto = new BCCryptoProvider();

		/// <summary>
		/// Test symmetric encryption, including padding.
		/// </summary>
		[Test]
		public void SymmetricTest()
		{
			Task.WaitAll(SymmetricTestAsync());
		}

		public async Task SymmetricTestAsync()
		{
			const int STEP = 30;
			byte[] key = await crypto.GetRandom(CryptoConstants.SYMMETRIC_KEY_LENGTH);
			byte[] iv  = await crypto.GetRandom(CryptoConstants.IV_LENGTH);
			List<byte> data = new List<byte>();
			for (int i = 0; i <= CryptoConstants.SYMMETRIC_BLOCK_LENGTH * 3; i += STEP)
			{
				data.AddRange(await crypto.GetRandom(STEP));
				byte[] d = data.ToArray();
				byte[] encrypted = await crypto.EncryptSymetric(key, iv, d);
				byte[] unencrypted = await crypto.DecryptSymetric(key, iv, encrypted);
				Assert.True(d.SequenceEqual(unencrypted));
			}
			data.Clear();
			for (int i = 0; i <= CryptoConstants.SYMMETRIC_BLOCK_LENGTH * 4; i += CryptoConstants.SYMMETRIC_BLOCK_LENGTH)
			{
				data.AddRange(await crypto.GetRandom(STEP));
				byte[] d = data.ToArray();
				byte[] encrypted = await crypto.EncryptSymetric(key, iv, d);
				byte[] unencrypted = await crypto.DecryptSymetric(key, iv, encrypted);
				Assert.True(d.SequenceEqual(unencrypted));
			}
		}

		/// <summary>
		/// Test asymmetric signation and verification.
		/// </summary>
		[Test]
		public void AsymmetricTest()
		{
			Task.WaitAll(AsymmetricTestAsync());
		}

		public async Task AsymmetricTestAsync()
		{
			KeyPair key = await crypto.CreateAsymmetricKey();
			const int STEP = 30;
			List<byte> data = new List<byte>();
			for (int i = 0; i <= CryptoConstants.SYMMETRIC_BLOCK_LENGTH * 3; i += STEP)
			{
				data.AddRange(await crypto.GetRandom(STEP));
				byte[] d = data.ToArray();
				byte[] signature = await crypto.Sign(key, d);
				Assert.True(await crypto.Verify(key, signature, d));
			}
		}
	}
}
