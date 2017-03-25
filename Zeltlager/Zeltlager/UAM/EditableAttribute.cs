using System;

namespace Zeltlager.UAM
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class EditableAttribute : Attribute
	{
		/// <summary>
		/// The name that should be displayed (how the property is called for the user)
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether an attribute can also be left empty.
		/// </summary>
		/// <value><c>true</c> if value of the attribute can be empty; otherwise, <c>false</c>.</value>
		public bool CanBeEmpty { get; set; }

		/// <summary>
		/// The name of a property that extends the static Name value.
		/// At the moment, this can only be used for classes, to set a dynamic page title.
		/// </summary>
		public string NameProperty { get; set; }

		public EditableAttribute(string name)
		{
			Name = name;
		}

		public EditableAttribute(string name, bool canBeEmpty)
		{
			Name = name;
			CanBeEmpty = canBeEmpty;
		}
	}
}
