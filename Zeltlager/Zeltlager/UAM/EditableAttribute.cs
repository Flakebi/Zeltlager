using System;

namespace Zeltlager.UAM
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class EditableAttribute : Attribute
	{
		/// <summary>
		/// The name that should be displayed (how the property is called for the user)
		/// </summary>
		public string Name { get; set; }

		public EditableAttribute(string name)
		{
			Name = name;
		}
	}
}
