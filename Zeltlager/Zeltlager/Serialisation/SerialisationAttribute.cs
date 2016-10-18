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
		/// If an object has fields that are marked as id,
		/// if must have the following static method for deserialisation:
		/// GetFromId(...)
		/// GetFromId must take all fields that are marked as id as
		/// parameters. It can also take a serialisation context.
		/// </summary>
		Id,
	}

	/// <summary>
	/// This attribute marks fields and properties for serialisation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class SerialisationAttribute : Attribute
	{
		/// <summary>
		/// The type of this attribute: How it should be serialised.
		/// Default value: Full
		/// </summary>
		public SerialisationType Type { get; set; }

		/// <summary>
		/// If this attribute is optional.
		/// Default value: False
		/// </summary>
		public bool Optional { get; set; }

		public SerialisationAttribute()
		{
			Type = SerialisationType.Full;
			Optional = false;
		}
	}
}
