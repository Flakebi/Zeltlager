using System;
using Xamarin.Forms;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Zeltlager.UAM
{
	public class UniversalAddModifyPage<T> : ContentPage where T : IEditable<T>
	{
		public T Obj { get; }
		private T oldObj;
		private bool isAddPage;

		static readonly Type[] numtypes = { typeof(ushort), typeof(int), typeof(byte) };

		public UniversalAddModifyPage(T obj, bool isAddPage)
		{
			// set title of page
			this.isAddPage = isAddPage;
			if (isAddPage)
				Title = obj.GetType().GetTypeInfo().GetCustomAttribute<EditableAttribute>().Name + " hinzufügen";
			else
				Title = obj.GetType().GetTypeInfo().GetCustomAttribute<EditableAttribute>().Name + " bearbeiten";

			var grid = new Grid();
			// add two columns to grid (labeling the input elements and the elements themselves)
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

			this.oldObj = obj;

			// save old object so we can delete it when save is clicked
			this.Obj = oldObj.CloneDeep();

			Type type = Obj.GetType();
			IEnumerable<PropertyInfo> propInfo = type.GetRuntimeProperties();

			// counting in which attribute we are
			int attributeNumber = 0;
			foreach (PropertyInfo pi in propInfo.Where(pi => pi.GetCustomAttribute<EditableAttribute>() != null))
			{
				// new row for every editable attribute
				grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

				Label label = new Label
				{
					Text = pi.GetCustomAttribute<EditableAttribute>().Name + " :"
				};
				grid.Children.Add(label, 0, attributeNumber);

				// create input element matching the type of the current attribute
				View manip = new Button();
				Type vartype = pi.PropertyType;

				// set the binding context so the binding of variables work
				BindingContext = Obj;

				if (vartype == typeof(string))
				{
					// use text entry
					manip = new Entry
					{
						Keyboard = Keyboard.Text
					};
					manip.SetBinding(Entry.TextProperty, new Binding(pi.Name, BindingMode.TwoWay));
				}
				else if (vartype == typeof(DateTime))
				{
					// use date picker
					manip = new DatePicker();
					manip.SetBinding(DatePicker.DateProperty, new Binding(pi.Name, BindingMode.TwoWay));
				}
				else if (vartype == typeof(TimeSpan))
				{
					// use time picker
					manip = new TimePicker();
					manip.SetBinding(TimePicker.TimeProperty, new Binding(pi.Name, BindingMode.TwoWay));
				}
				else if (numtypes.Contains(vartype))
				{
					// use entry with num Keyboard
					manip = new Entry
					{
						Keyboard = Keyboard.Numeric
					};
					manip.SetBinding(Entry.TextProperty, new Binding(pi.Name, BindingMode.TwoWay));
				}
				else if (vartype == typeof(Tent))
				{
					// use picker filled with all tents
					Picker picker = new Picker();
					foreach (Tent tent in Lager.CurrentLager.Tents) 
					{
						picker.Items.Add(tent.ToString());
					}
					picker.SelectedIndexChanged += (sender, args) =>
					{
						Tent t = null;
						// find correct tent from display string
						foreach (Tent tent in Lager.CurrentLager.Tents)
						{
							if (tent.Display == picker.Items[picker.SelectedIndex])
							{
								t = tent;
								break;
							}
						}
						type.GetRuntimeProperty(pi.Name).SetValue(Obj, t, null);
					};
					manip = picker;
				}
				else if (vartype == typeof(bool))
				{
					// use switch
					manip = new Switch();
					manip.SetBinding(Switch.IsToggledProperty, new Binding(pi.Name, BindingMode.TwoWay));
				}
				else if (vartype == typeof(List<object>))
				{
					// use list edit
					// TODO: Write List edit
				}
				else
				{
					throw new Exception("Type " + vartype + " not supported by UniversalAddModifyPage");
				}

				grid.Children.Add(manip, 1, attributeNumber);

				attributeNumber++;
			}

			Content = grid;
			ToolbarItems.Add(new ToolbarItem("Abbrechen", null, OnCancelClicked, ToolbarItemOrder.Primary, 0));
			ToolbarItems.Add(new ToolbarItem("Speichern", null, OnSaveClicked, ToolbarItemOrder.Primary, 1));
			Style = (Style)Application.Current.Resources["BaseStyle"];
			// make page not start directly at the top
			Padding = new Thickness(8, 15, 8, 0);
		}

		private void OnCancelClicked()
		{
			Navigation.PopModalAsync(true);
		}

		private void OnSaveClicked()
		{
			if (isAddPage)
				oldObj = default(T);
			Obj.OnSaveEditing(oldObj);
			Navigation.PopModalAsync(true);
		}
	}
}

