using NUnit.Framework;

using Zeltlager;

namespace UnitTests
{
	[TestFixture]
	public class HelperTests
	{
		[Test]
		public void ToBytesSize()
		{
			int i = 42;
			Assert.AreEqual(sizeof(int), i.ToBytes().Length);
			i = 0;
			Assert.AreEqual(sizeof(int), i.ToBytes().Length);
			uint ui = 42;
			Assert.AreEqual(sizeof(uint), ui.ToBytes().Length);
			ui = 0;
			Assert.AreEqual(sizeof(uint), ui.ToBytes().Length);

			short s = 42;
			Assert.AreEqual(sizeof(short), s.ToBytes().Length);
			s = 0;
			Assert.AreEqual(sizeof(short), s.ToBytes().Length);
		}

		[Test]
		public void ToFromBytes()
		{
			int i = 42;
			Assert.AreEqual(i, i.ToBytes().ToInt(0));
			i = 0;
			Assert.AreEqual(i, i.ToBytes().ToInt(0));
			uint ui = 42;
			Assert.AreEqual(ui, ui.ToBytes().ToUInt(0));
			ui = 0;
			Assert.AreEqual(ui, ui.ToBytes().ToUInt(0));
		}
	}
}
