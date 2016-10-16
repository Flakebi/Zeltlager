using System;

namespace Zeltlager.UAM
{
	public class EditableAttribute : Attribute
	{
		// The name that should be displayed (how the property is called for the user)
		private string name;
		public string Name { get { return name; } set { name = value; } }

		public EditableAttribute(string name)
		{
			this.name = name;
		}
	}
}
