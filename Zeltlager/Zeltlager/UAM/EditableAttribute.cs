using System;
namespace Zeltlager.UAM
{
	public class EditableAttribute : Attribute, IComparable
	{
		// the name that should be displayed (how the property is called for the user)
		private string name;
		public String Name { get { return name; } set { name = value; } }

		public EditableAttribute(string name)
		{
			this.name = name;
		}

		public int CompareTo(object obj)
		{
			if (obj.GetType() == this.GetType())
				return 0;
			else
				return 1;
		}
	}
}

