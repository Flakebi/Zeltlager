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

	/// <summary>
	/// This attribute marks fields and properties for serialisation.
	/// An object that should be deserialised must have a constructor
	/// that takes all fields as arguments that are marked for serialisation,
	/// except the ids which have to be initialised by the constructor.
	/// For that purpose you can take a SerialisationContext that contains
	/// a lager and the collaborator that created the currenty deserialised
	/// packet.
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
