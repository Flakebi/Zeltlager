using System;
using Zeltlager.Serialisation;

namespace Zeltlager
{
	public interface IDeletable
	{
		[Serialisation]
		bool IsVisible { get; set; }
	}
}
