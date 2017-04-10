using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Xamarin.Forms;
using Zeltlager.Competition;
using Zeltlager.Erwischt;

namespace Zeltlager.UAM
{
	using Client;

	/// <summary>
	/// A page to manipulate annotated properties from any given type.
	/// Supports these types of properties:
	/// string, DateTime, TimeSpan, any Number, Tent, Member, Partipant, bool, List
	/// </summary>
	public class UniversalAddModifyPage<T, U> : ContentPage where T : IEditable<U>, U
	{
		public T Obj { get; }
		T oldObj;
		readonly bool isAddPage;
		readonly LagerClient lager;

		static readonly Type[] NUM_TYPES = {
			typeof(byte),
			typeof(sbyte),
			typeof(ushort),
			typeof(short),
			typeof(uint),
			typeof(int),
			typeof(ulong),
			typeof(long),
			typeof(int?),
		};

		static readonly Type[] OWN_TYPES = {
			typeof(Member),
			typeof(Tent),
			typeof(Participant),
		};

		public UniversalAddModifyPage(T obj, bool isAddPage, LagerClient lager)
		{
			this.lager = lager;
			this.isAddPage = isAddPage;
			Type type = typeof(T);

			// Set title of page
			EditableAttribute classAttribute = type.GetTypeInfo().GetCustomAttribute<EditableAttribute>();
			Title = classAttribute.Name +
				(isAddPage ? " hinzufügen" : " bearbeiten");
			// Add a dynamic title if one is give
			if (classAttribute.NameProperty != null)
				Title += " - " + type.GetRuntimeProperty(classAttribute.NameProperty).GetValue(obj);

			var grid = new Grid();
			// Add two columns to grid (labeling the input elements and the elements themselves)
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

			// Save old object so we can delete it when save is clicked
			oldObj = obj;

			Obj = (T)obj.Clone();

			// Set the binding context so the binding of variables work
			BindingContext = Obj;
			
			IEnumerable<PropertyInfo> propInfo = type.GetRuntimeProperties()
			                                         .Where(pi => pi.GetCustomAttribute<EditableAttribute>() != null);

			// Counting in which attribute we are
			int attributeNumber = 0;
			foreach (PropertyInfo pi in propInfo)
			{
				// new row for every editable attribute
				grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

				Label label = new Label
				{
					Text = pi.GetCustomAttribute<EditableAttribute>().Name + ": ",
					TextColor = (Color) Application.Current.Resources["textColorSecondary"]
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
					var tp = new TimePicker();
					tp.SetBinding(TimePicker.TimeProperty, new Binding(pi.Name, BindingMode.TwoWay));
					manip = tp;
				}

				else if (NUM_TYPES.Contains(vartype))
				{
					// use entry with num Keyboard
					Entry entry = new Entry
					{
						Keyboard = Keyboard.Numeric,
					};
					object text = type.GetRuntimeProperty(pi.Name).GetValue(Obj) ?? string.Empty;
					entry.Text = text.ToString();
					entry.TextChanged += (sender, e) =>
					{
						object[] parameters = { ((Entry)sender).Text, null };
						if (vartype.IsConstructedGenericType && vartype.GetGenericTypeDefinition() == typeof(Nullable<>))
						{
							if (string.IsNullOrEmpty(((Entry)sender).Text))
							{
								type.GetRuntimeProperty(pi.Name).SetValue(Obj, null);
								return;
							}
							vartype = vartype.GenericTypeArguments[0];
						}
						if ((bool) vartype.GetTypeInfo().GetDeclaredMethods("TryParse").First
															(x => x.GetParameters().Length == 2).Invoke(null, parameters))
						{
							//type.GetRuntimeProperty(pi.Name).SetValue(Obj, Helpers.ConvertParam(((Entry)sender).Text, vartype));
							type.GetRuntimeProperty(pi.Name).SetValue(Obj, parameters[1]);
						}
					};
					manip = entry;
				}

				else if (OWN_TYPES.Contains(vartype))
				{
					// use picker filled with all members
					Picker picker = new Picker();
					// var typeForList = typeof(IReadOnlyList<>).MakeGenericType(vartype);
					IReadOnlyList<object> list = (IReadOnlyList<object>)type.GetRuntimeProperty(pi.Name + "List").GetValue(Obj);
					foreach (object o in list)
					{
						picker.Items.Add(o.ToString());
					}
					picker.SelectedIndexChanged += (sender, args) =>
					{
						object o = vartype.GetTypeInfo().GetDeclaredMethod("GetFromString").Invoke
													(null, new object[] { lager, picker.Items[picker.SelectedIndex] });
						type.GetRuntimeProperty(pi.Name).SetValue(Obj, o);
					};
					picker.SelectedIndex = 0;
					manip = picker;
				}

				else if (vartype == typeof(bool))
				{
					// use switch
					var sw = new Switch();
					sw.SetBinding(Switch.IsToggledProperty, new Binding(pi.Name, BindingMode.TwoWay));
					manip = sw;
				}

				else if (vartype.GetTypeInfo().IsGenericType && vartype.GetGenericTypeDefinition() == typeof(List<>))
				{
					// use list edit
					var genericListType = vartype.GenericTypeArguments[0];
					var typeForListEdit = typeof(ListEditPage<>).MakeGenericType(genericListType);
					var currentValue = type.GetRuntimeProperty(pi.Name).GetValue(Obj);
					object listEditPage = null;
					if (genericListType == typeof(Member))
					{
						IReadOnlyList<Member> list = (IReadOnlyList<Member>)type.GetRuntimeProperty(pi.Name + "List").GetValue(Obj);
						listEditPage = typeForListEdit.GetTypeInfo().DeclaredConstructors.First().Invoke(new object[] { list, currentValue, Title });
					}
					Button b = new Button
					{
						Text = pi.GetCustomAttribute<EditableAttribute>().Name + " bearbeiten",
						Style = (Style)Application.Current.Resources["DarkButtonStyle"]
					};
					b.Clicked += (sender, e) =>
					{
						Navigation.PushModalAsync(new NavigationPage((Page)listEditPage));
					};
					manip = b;
				} 

				else
				{
					throw new InvalidOperationException("Type " + vartype + " is not supported by UniversalAddModifyPage");
				}

				grid.Children.Add(manip, 1, attributeNumber);

				attributeNumber++;
			}

			Content = grid;
			ToolbarItems.Add(new ToolbarItem(null, Icons.CANCEL, OnCancelClicked, ToolbarItemOrder.Primary, 0));
			ToolbarItems.Add(new ToolbarItem(null, Icons.SAVE, OnSaveClicked, ToolbarItemOrder.Primary, 1));
			Style = (Style)Application.Current.Resources["BaseStyle"];
			// make page not start directly at the top
			Padding = new Thickness(10);
			NavigationPage.SetBackButtonTitle(this, "");
		}

		void OnCancelClicked()
		{
			Navigation.PopAsync(true);
		}

		async void OnSaveClicked()
		{
			// check if any string properties are empty
			PropertyInfo propInfo = typeof(T).GetRuntimeProperties()
			   .FirstOrDefault(pi => pi.GetCustomAttribute<EditableAttribute>() != null 
			                   && pi.PropertyType == typeof(string) 
			                   && string.IsNullOrEmpty((string)typeof(T).GetRuntimeProperty(pi.Name).GetValue(Obj)));
			if (propInfo != null && !propInfo.GetCustomAttribute<EditableAttribute>().CanBeEmpty)
			{
				await DisplayAlert("Achtung!", propInfo.GetCustomAttribute<EditableAttribute>().Name + " erforderlich.", "Ok");
				return;
			}
			// end of check

			if (isAddPage)
				oldObj = default(T);
			await Obj.OnSaveEditing(lager, oldObj);

			if (Obj is ErwischtGame)
			{
				Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count-2]);
				Navigation.InsertPageBefore(new ErwischtPage(lager.ErwischtHandler.GetNewestGame(), lager), this);
			}

			await Navigation.PopAsync();
		}
	}
}
