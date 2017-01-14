using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager
{
	public static class Icons
	{
		public const string EDIT_TEXT = "✏️";
		public const string DELETE_TEXT = "\ud83d\uddd1";
		public const string SETTINGS_TEXT = "⚙";
		public const string ADD_TEXT = "＋";
		public const string SAVE_TEXT = "\ud83d\udcbe";
		public const string CANCEL_TEXT = "❌";

		public const string ADD = "add.png";
		public const string ARROW_LEFT = "leftarrow1.png";
		public const string ARROW_RIGHT = "rightarrow1.png";
		public const string CANCEL = "cancel1.png";
		public const string DELETE = "garbage.png";
		public const string EDIT = "edit.png";
		public const string SAVE = "correct.png";
		public const string SETTINGS = "settings1.png";
		public const string PODIUM = "podium.png";
		public const string TIMER = "timer.png";
		public const string FOOD = "cutlery.png";
		public const string PLAN = "waiting.png";

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
			{
				int newIndex = collection.IndexOf(sorted[i]);
				if (newIndex != i)
					collection.Move(newIndex, i);
			}
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
			byte[] bytes = new byte[sizeof(ushort)];
			value.ToBytes(bytes, 0);
			return bytes;
		}

		public static byte[] ToBytes(this short value)
		{
			byte[] bytes = new byte[sizeof(short)];
			value.ToBytes(bytes, 0);
			return bytes;
		}

		public static byte[] ToBytes(this uint value)
		{
			byte[] bytes = new byte[sizeof(uint)];
			value.ToBytes(bytes, 0);
			return bytes;
		}

		public static byte[] ToBytes(this int value)
		{
			byte[] bytes = new byte[sizeof(int)];
			value.ToBytes(bytes, 0);
			return bytes;
		}

		public static byte[] ToBytes(this ulong value)
		{
			byte[] bytes = new byte[sizeof(ulong)];
			value.ToBytes(bytes, 0);
			return bytes;
		}

		public static byte[] ToBytes(this long value)
		{
			byte[] bytes = new byte[sizeof(long)];
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

		public static string ToHexString(this byte[] bytes)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var b in bytes)
				sb.Append(b.ToString("X2"));
			return sb.ToString();
		}

		public static async Task<byte[]> ReadAsyncSafe(this Stream stream, int count)
		{
			byte[] result = new byte[count];
			do
			{
				int read = await stream.ReadAsync(result, result.Length - count, count);
				if (read == 0)
					throw new EndOfStreamException("Unexpected end of stream while reading async");
				count -= read;
			} while (count > 0);
			return result;
		}

		public static object ConvertParam(string value, Type targetType)
		{
			if (targetType == typeof(string))
				return value;
			if (targetType.IsConstructedGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
				targetType = targetType.GenericTypeArguments[0];

			return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
		}

		public static int CompareTo(this int? number, int? other)
		{
			if (!number.HasValue || !other.HasValue)
			{
				if (!other.HasValue)
				{
					if (!number.HasValue)
					{
						return 0;	
					} 
					else
					{
						return 1;
					}
				}
				return -1;
			}
			return number.Value.CompareTo(other.Value);
		}
	}
}
