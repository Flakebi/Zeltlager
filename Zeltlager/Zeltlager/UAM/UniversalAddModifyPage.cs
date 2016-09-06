using System;
using Xamarin.Forms;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Zeltlager
{
	public class UniversalAddModifyPage<T> : ContentPage
	{
		public IEditable<T> Obj { get; }

		public UniversalAddModifyPage(IEditable<T> obj)
		{
			// init surrounding GUI
			StackLayout vsl = new StackLayout
			{
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};

			this.Obj = obj;
			Type t = obj.GetType();
			IEnumerable<PropertyInfo> propInfo = t.GetRuntimeProperties();

			foreach (PropertyInfo pi in propInfo)
			{
				if (pi.GetCustomAttributes().Contains(new EditableAttribute("")))
				{
					StackLayout hsl = new StackLayout
					{
						Orientation = StackOrientation.Horizontal
					};
					Label label = new Label
					{
						Text = ((EditableAttribute)pi.GetCustomAttributes().First()).Name
					};

					View manip;
					Type vartype = pi.GetType();
					if (vartype == typeof(string))
					{
						// use text entry
					}
					else if (vartype == typeof(DateTime))
					{
						// use date picker
					}
				}
			}

			Content = vsl;
		}
	}
}

