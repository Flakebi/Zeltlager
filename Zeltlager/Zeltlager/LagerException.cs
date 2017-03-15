using System;

namespace Zeltlager
{
	public class LagerException : Exception
	{
		public LagerException() { }
		public LagerException(string message) : base(message) { }
		public LagerException(string message, Exception innerException) : base(message, innerException) { }
	}
}
