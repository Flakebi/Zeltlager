using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Zeltlager
{
	public static class Icons
	{
		public const string EDIT = "✏️";
		public const string DELETE = "\ud83d\uddd1";
		public const string SETTINGS = "⚙";
		public const string ADD = "＋";

		public static readonly Dictionary<DayOfWeek, string> WEEKDAYS = new Dictionary<DayOfWeek, string>
		{
			{DayOfWeek.Monday, "\ud83c\udf19"},
			{DayOfWeek.Tuesday, "\ud83d\udd25"},
			{DayOfWeek.Wednesday, "\ud83c\udf0a"},
			{DayOfWeek.Thursday, "\ud83c\udf33"},
			{DayOfWeek.Friday, "\ud83c\udfc5"},
			{DayOfWeek.Saturday, "\ud83c\udf0d"},
			{DayOfWeek.Sunday, "☀️"},
		};
	}

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

		public static void ToBytes(this short value, byte[] array, int offset) => ((ushort)value).ToBytes(array, offset);

		public static void ToBytes(this uint value, byte[] array, int offset)
		{
			array[offset] = (byte)(value >> 24);
			array[offset + 1] = (byte)(value >> 16);
			array[offset + 2] = (byte)(value >> 8);
			array[offset + 3] = (byte)value;
		}

		public static void ToBytes(this int value, byte[] array, int offset) => ((uint)value).ToBytes(array, offset);

		public static void ToBytes(this ulong value, byte[] array, int offset)
		{
			array[offset] = (byte)(value >> 56);
			array[offset + 1] = (byte)(value >> 48);
			array[offset + 2] = (byte)(value >> 40);
			array[offset + 3] = (byte)(value >> 32);
			array[offset + 4] = (byte)(value >> 24);
			array[offset + 5] = (byte)(value >> 16);
			array[offset + 6] = (byte)(value >> 8);
			array[offset + 7] = (byte)value;
		}

		public static void ToBytes(this long value, byte[] array, int offset) => ((ulong)value).ToBytes(array, offset);

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

		public static byte[] ToBytes(this ulong value)
		{
			byte[] bytes = new byte[8];
			value.ToBytes(bytes, 0);
			return bytes;
		}

		public static byte[] ToBytes(this long value)
		{
			byte[] bytes = new byte[8];
			value.ToBytes(bytes, 0);
			return bytes;
		}

		public static ushort ToUShort(this byte[] value, int offset) => (ushort)((value[offset] << 8) | value[offset + 1]);

		public static short ToShort(this byte[] value, int offset) => (short)value.ToUShort(offset);

		public static uint ToUInt(this byte[] value, int offset)
		{
			return ((uint)value[offset] << 24)
				| ((uint)value[offset + 1] << 16)
				| ((uint)value[offset + 2] << 8)
				| value[offset + 3];
		}

		public static int ToInt(this byte[] value, int offset) => (int)value.ToUInt(offset);

		public static ulong ToULong(this byte[] value, int offset)
		{
			return ((ulong)value[offset] << 56)
				| ((ulong)value[offset + 1] << 48)
				| ((ulong)value[offset + 2] << 40)
				| ((ulong)value[offset + 3] << 32)
				| ((ulong)value[offset + 4] << 24)
				| ((ulong)value[offset + 5] << 16)
				| ((ulong)value[offset + 6] << 8)
				| value[offset + 7];
		}

		public static long ToLong(this byte[] value, int offset) => (long)value.ToULong(offset);
	}
}
