using System;
namespace Zeltlager
{
	public class EditableAttribute : Attribute, IComparable
	{
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

