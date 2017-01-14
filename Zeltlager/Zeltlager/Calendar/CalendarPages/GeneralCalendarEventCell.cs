using System;
using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	public class GeneralCalendarEventCell : ViewCell
	{
		#region Bindable Properties

		/// <summary>
		/// The bindable property implementation
		/// </summary>
		public static readonly BindableProperty OnEditCommandParameterProperty = BindableProperty.Create(nameof(OnEditCommandParameter), typeof(object), typeof(SearchableCell), null);
		/// <summary>
		/// The edit command parameter
		/// </summary>
		public object OnEditCommandParameter
		{
			get { return GetValue(OnEditCommandParameterProperty); }
			set
			{
				SetValue(OnEditCommandParameterProperty, value);
				OnPropertyChanged();
			}
		}
		/// <summary>
		/// The bindable property implementation
		/// </summary>
		public static readonly BindableProperty OnEditCommandProperty = BindableProperty.Create(nameof(OnEditCommand), typeof(Command), typeof(SearchableCell), null);
		/// <summary>
		/// The command which gets executed on selecting edit on the cell
		/// </summary>
		public Command OnEditCommand
		{
			get { return (Command)GetValue(OnEditCommandProperty); }
			set
			{
				SetValue(OnEditCommandProperty, value);
				OnPropertyChanged();
			}
		}

		public static readonly BindableProperty OnDeleteCommandParameterProperty = BindableProperty.Create(nameof(OnDeleteCommandParameter), typeof(object), typeof(SearchableCell), null);
		/// <summary>
		/// The delete command parameter
		/// </summary>
		public object OnDeleteCommandParameter
		{
			get { return GetValue(OnDeleteCommandParameterProperty); }
			set
			{
				SetValue(OnEditCommandParameterProperty, value);
				OnPropertyChanged();
			}
		}
		/// <summary>
		/// The bindable property implementation
		/// </summary>
		public static readonly BindableProperty OnDeleteCommandProperty = BindableProperty.Create(nameof(OnDeleteCommand), typeof(Command), typeof(SearchableCell), null);
		/// <summary>
		/// The command which gets executed on selecting delete onthe cell
		/// </summary>
		public Command OnDeleteCommand
		{
			get { return (Command)GetValue(OnDeleteCommandProperty); }
			set
			{
				SetValue(OnDeleteCommandProperty, value);
				OnPropertyChanged();
			}
		}

		#endregion

		MenuItem editAction, deleteAction;
		Label time, title, detail;

		public GeneralCalendarEventCell()
		{
			time = new Label
			{
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalTextAlignment = TextAlignment.End,
				TextColor = (Color)Application.Current.Resources["whiteColor"]
			};
			title = new Label
			{
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalTextAlignment = TextAlignment.End
			};
			detail = new Label
			{
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalTextAlignment = TextAlignment.End,
				TextColor = (Color)Application.Current.Resources["textColorSecondary"]
			};

			StackLayout horizontalLayout = new StackLayout();

			horizontalLayout.Padding = new Thickness(10, 0);

			//set bindings
			time.SetBinding(Label.TextProperty, "TimeString");
			title.SetBinding(Label.TextProperty, "Title");
			detail.SetBinding(Label.TextProperty, "Detail");


			horizontalLayout.Orientation = StackOrientation.Horizontal;
			time.HorizontalOptions = LayoutOptions.Start;

			horizontalLayout.Children.Add(time);
			horizontalLayout.Children.Add(title);
			horizontalLayout.Children.Add(detail);

			editAction = new MenuItem { Icon = Icons.EDIT, Text = Icons.EDIT_TEXT };
			editAction.SetBinding(MenuItem.CommandParameterProperty, new Binding(nameof(OnEditCommandParameter)));
			editAction.SetBinding(MenuItem.CommandProperty, new Binding(nameof(OnEditCommand)));
			editAction.BindingContext = this;

			deleteAction = new MenuItem { Icon = Icons.DELETE, Text = Icons.DELETE_TEXT, IsDestructive = true };
			deleteAction.SetBinding(MenuItem.CommandParameterProperty, new Binding(nameof(OnDeleteCommandParameter)));
			deleteAction.SetBinding(MenuItem.CommandProperty, new Binding(nameof(OnDeleteCommand)));
			deleteAction.BindingContext = this;

			ContextActions.Add(editAction);
			ContextActions.Add(deleteAction);

			View = horizontalLayout;
		}
	}
}
