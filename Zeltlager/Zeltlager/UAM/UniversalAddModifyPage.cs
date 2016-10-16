using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Xamarin.Forms;

namespace Zeltlager.UAM
{
	using Client;

	public class UniversalAddModifyPage<T> : ContentPage where T : IEditable<T>
	{
		public T Obj { get; }
		private T oldObj;
		private bool isAddPage;

		static readonly Type[] numtypes = { typeof(byte), typeof(ushort), typeof(int) };

		public UniversalAddModifyPage(T obj, bool isAddPage)
		{
			// Set title of page
			this.isAddPage = isAddPage;
			Title = obj.GetType().GetTypeInfo().GetCustomAttribute<EditableAttribute>().Name +
				(isAddPage ? " hinzufügen" : " bearbeiten");

			var grid = new Grid();
			// Add two columns to grid (labeling the input elements and the elements themselves)
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

			oldObj = obj;

			// Save old object so we can delete it when save is clicked
			Obj = oldObj.CloneDeep();

			// Set the binding context so the binding of variables work
			BindingContext = Obj;

			Type type = Obj.GetType();
			IEnumerable<PropertyInfo> propInfo = type.GetRuntimeProperties().Where(pi => pi.GetCustomAttribute<EditableAttribute>() != null);

			// Counting in which attribute we are
			int attributeNumber = 0;
			foreach (PropertyInfo pi in propInfo)
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

				if (vartype == typeof(string))
				{
					// use text entry
					manip = new Entry
					{
						Keyboard = Keyboard.Text
					};
					manip.SetBinding(Entry.TextProperty, new Binding(pi.Name, BindingMode.TwoWay));
				} else if (vartype == typeof(DateTime))
				{
					// use date picker
					manip = new DatePicker();
					manip.SetBinding(DatePicker.DateProperty, new Binding(pi.Name, BindingMode.TwoWay));
				} else if (vartype == typeof(TimeSpan))
				{
					// use time picker
					var tp = new TimePicker();
					tp.SetBinding(TimePicker.TimeProperty, new Binding(pi.Name, BindingMode.TwoWay));
					manip = tp;
				} else if (numtypes.Contains(vartype))
				{
					// use entry with num Keyboard
					manip = new Entry
					{
						Keyboard = Keyboard.Numeric
					};
					manip.SetBinding(Entry.TextProperty, new Binding(pi.Name, BindingMode.TwoWay));
				} else if (vartype == typeof(Tent))
				{
					// use picker filled with all tents
					Picker picker = new Picker();
					foreach (Tent tent in LagerClient.CurrentLager.Tents)
					{
						picker.Items.Add(tent.ToString());
					}
					picker.SelectedIndexChanged += (sender, args) =>
					{
						Tent t = null;
						// find correct tent from display string
						foreach (Tent tent in LagerClient.CurrentLager.Tents)
						{
							if (tent.Display == picker.Items[picker.SelectedIndex])
							{
								t = tent;
								break;
							}
						}
						type.GetRuntimeProperty(pi.Name).SetValue(Obj, t);
					};
					picker.SelectedIndex = 0;
					manip = picker;
				} else if (vartype == typeof(bool))
				{
					// use switch
					var sw = new Switch();
					//sw.IsToggled = (bool) type.GetRuntimeProperty(pi.Name).GetValue(Obj);
					sw.SetBinding(Switch.IsToggledProperty, new Binding(pi.Name, BindingMode.TwoWay));
					manip = sw;
				} else if (vartype == typeof(List<object>))
				{
					// use list edit
					// TODO: Write List edit
				} else
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

		async void OnSaveClicked()
		{
			if (isAddPage)
				oldObj = default(T);
			await Obj.OnSaveEditing(oldObj);
			await Navigation.PopModalAsync(true);
		}
	}
}
