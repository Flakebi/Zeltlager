using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Xamarin.Forms;
using Zeltlager.Competition;

namespace Zeltlager.UAM
{
	using System.Collections;
	using Client;
	using DataPackets;
	using Serialisation;

	/// <summary>
	/// A page to manipulate annotated properties from any given type.
	/// Supports these types of properties:
	/// string, DateTime, TimeSpan, any Number, Tent, Member, Partipant, bool, List
	/// </summary>

	public class UniversalAddModifyPage<T> : ContentPage where T : IEditable<T>
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

		public UniversalAddModifyPage(T obj, bool isAddPage, LagerClient lager)
		{
			this.lager = lager;
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
			Obj = oldObj.Clone();

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
						type.GetRuntimeProperty(pi.Name).SetValue(Obj, Helpers.ConvertParam(((Entry)sender).Text, vartype));
					};
					manip = entry;
				}

				else if (vartype == typeof(Tent))
				{
					// use picker filled with all tents
					Picker picker = new Picker();
					IReadOnlyList<Tent> list = (IReadOnlyList<Tent>) type.GetRuntimeProperty(pi.Name + "List").GetValue(Obj);
					foreach (Tent tent in list)
					{
						picker.Items.Add(tent.ToString());
					}
					picker.SelectedIndexChanged += (sender, args) =>
					{
						Tent t = lager.GetTentFromDisplay(picker.Items[picker.SelectedIndex]);
						type.GetRuntimeProperty(pi.Name).SetValue(Obj, t);
					};
					picker.SelectedIndex = 0;
					manip = picker;
				}

				else if (vartype == typeof(Member))
				{
					// use picker filled with all members
					Picker picker = new Picker();
					IReadOnlyList<Member> list = (IReadOnlyList<Member>) type.GetRuntimeProperty(pi.Name + "List").GetValue(Obj);
					foreach (Member mem in list)
					{
						picker.Items.Add(mem.ToString());
					}
					picker.SelectedIndexChanged += (sender, args) =>
					{
						Member m = lager.GetMemberFromString(picker.Items[picker.SelectedIndex]);
						type.GetRuntimeProperty(pi.Name).SetValue(Obj, m);
					};
					picker.SelectedIndex = 0;
					manip = picker;
				}

				else if (vartype == typeof(Participant))
				{
					// use picker filled with all members
					Picker picker = new Picker();
					IReadOnlyList<Participant> list = (IReadOnlyList<Participant>)type.GetRuntimeProperty(pi.Name + "List").GetValue(Obj);
					foreach (Participant par in list)
					{
						picker.Items.Add(par.Name);
					}
					picker.SelectedIndexChanged += (sender, args) =>
					{
						Participant p = lager.CompetitionHandler.GetParticipantFromName(picker.Items[picker.SelectedIndex]);
						type.GetRuntimeProperty(pi.Name).SetValue(Obj, p);
					};
					picker.SelectedIndex = 0;
					manip = picker;
				}

				else if (vartype == typeof(bool))
				{
					// use switch
					var sw = new Switch();
					//sw.IsToggled = (bool) type.GetRuntimeProperty(pi.Name).GetValue(Obj);
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
					throw new Exception("Type " + vartype + " not supported by UniversalAddModifyPage");
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
		}

		void OnCancelClicked()
		{
			Navigation.PopModalAsync(true);
		}

		async void OnSaveClicked()
		{
			if (isAddPage)
				oldObj = default(T);
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(lager.Manager, lager);
			context.PacketId = new PacketId(lager.OwnCollaborator);
			await Obj.OnSaveEditing(lager.ClientSerialiser, context, oldObj);
			await Navigation.PopModalAsync(true);
		}
	}
}
