using System;

namespace Zeltlager.Serialisation
{
	/// <summary>
	/// How the described field or property should be saved.
	/// </summary>
	public enum SerialisationType
	{
		/// <summary>
		/// The field should be serialised completely.
		/// </summary>
		Full,
		/// <summary>
		/// Only the id of this field should be saved.
		/// </summary>
		Reference,
		/// <summary>
		/// This field is an id and should be saved if
		/// other objects want to save references to this
		/// object.
		/// As ids should be unique, the next free id is
		/// stored for each collaborator in NextIds.
		/// </summary>
		Id,
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class SerialisationAttribute : Attribute
	{
		public SerialisationType Type { get; set; }

		public SerialisationAttribute()
		{
			Type = SerialisationType.Full;
		}
	}
}
