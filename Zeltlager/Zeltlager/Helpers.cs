using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Zeltlager
{
	public static class Helpers
	{
		public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable<T>
		{
			List<T> sorted = collection.OrderBy(x => x).ToList();
			for (int i = 0; i < sorted.Count(); i++)
				collection.Move(collection.IndexOf(sorted[i]), i);
		}

		public static void ToBytes(this ushort value, byte[] array, int offset)
		{
			array[offset] = (byte)(value >> 8);
			array[offset + 1] = (byte)value;
		}

		public static void ToBytes(this short value, byte[] array, int offset)
		{
			((ushort)value).ToBytes(array, offset);
		}

		public static void ToBytes(this uint value, byte[] array, int offset)
		{
			array[offset] = (byte)(value >> 24);
			array[offset + 1] = (byte)(value >> 16);
			array[offset + 2] = (byte)(value >> 8);
			array[offset + 3] = (byte)value;
		}

		public static void ToBytes(this int value, byte[] array, int offset)
		{
			((uint)value).ToBytes(array, offset);
		}

		public static byte[] ToBytes(this ushort value)
		{
			byte[] bytes = new byte[2];
			value.ToBytes(bytes, 0);
			return bytes;
		}

		public static byte[] ToBytes(this short value)
		{
			byte[] bytes = new byte[2];
			value.ToBytes(bytes, 0);
			return bytes;
		}

		public static byte[] ToBytes(this uint value)
		{
			byte[] bytes = new byte[4];
			value.ToBytes(bytes, 0);
			return bytes;
		}

		public static byte[] ToBytes(this int value)
		{
			byte[] bytes = new byte[4];
			value.ToBytes(bytes, 0);
			return bytes;
		}

		public static ushort ToUShort(this byte[] value, int offset)
		{
			return (ushort)((value[offset] << 8) | value[offset + 1]);
		}

		public static short ToShort(this byte[] value, int offset)
		{
			return (short)value.ToUShort(offset);
		}

		public static uint ToUInt(this byte[] value, int offset)
		{
			return (uint)((value[offset] << 24)
				| (value[offset + 1] << 24)
				| (value[offset + 2] << 16)
				| value[offset + 3]);
		}

		public static int ToInt(this byte[] value, int offset)
		{
			return (int)value.ToUInt(offset);
		}
	}
}
