using System.ComponentModel;

using Xamarin.Forms;

namespace Zeltlager
{
	public class SearchableCell : TextCell
	{
		/// <summary>
		/// The bindable property implementation
		/// </summary>
		public static readonly BindableProperty OnEditCommandParameterProperty = BindableProperty.Create("OnEditCommandParameter", typeof(object), typeof(SearchableCell), null);
		/// <summary>
		/// The edit command parameter
		/// </summary>
		public object OnEditCommandParameter
		{
			get { return GetValue(OnEditCommandParameterProperty); }
			set
			{
				SetValue(OnEditCommandParameterProperty, value);
				OnPropertyChanged("OnEditCommandParameter");
			}
		}
		/// <summary>
		/// The bindable property implementation
		/// </summary>
		public static readonly BindableProperty OnEditCommandProperty = BindableProperty.Create("OnEditCommand", typeof(Command), typeof(SearchableCell), null);
		/// <summary>
		/// The command which gets executed on selecting edit on the cell
		/// </summary>
		public Command OnEditCommand
		{
			get { return (Command)GetValue(OnEditCommandProperty); }
			set
			{
				SetValue(OnEditCommandProperty, value);
				OnPropertyChanged("OnEditCommand");
			}
		}

		public static readonly BindableProperty OnDeleteCommandParameterProperty = BindableProperty.Create("OnDeleteCommandParameter", typeof(object), typeof(SearchableCell), null);
		/// <summary>
		/// The delete command parameter
		/// </summary>
		public object OnDeleteCommandParameter
		{
			get { return GetValue(OnDeleteCommandParameterProperty); }
			set
			{
				SetValue(OnEditCommandParameterProperty, value);
				OnPropertyChanged("OnDeleteCommandParameter");
			}
		}
		/// <summary>
		/// The bindable property implementation
		/// </summary>
		public static readonly BindableProperty OnDeleteCommandProperty = BindableProperty.Create("OnDeleteCommand", typeof(Command), typeof(SearchableCell), null);
		/// <summary>
		/// The command which gets executed on selecting delete onthe cell
		/// </summary>
		public Command OnDeleteCommand
		{
			get { return (Command)GetValue(OnDeleteCommandProperty); }
			set
			{
				SetValue(OnDeleteCommandProperty, value);
				OnPropertyChanged("OnDeleteCommand");
			}
		}

		public SearchableCell()
		{
			var editAction = new MenuItem { Text = Icons.EDIT };
			editAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("OnEditCommandParameter"));
			editAction.SetBinding(MenuItem.CommandProperty, new Binding("OnEditCommand"));
			editAction.BindingContext = this;

			var deleteAction = new MenuItem { Text = Icons.DELETE, IsDestructive = true };
			deleteAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("OnDeleteCommandParameter"));
			deleteAction.SetBinding(MenuItem.CommandProperty, new Binding("OnDeleteCommand"));
			editAction.BindingContext = this;

			ContextActions.Add(editAction);
			ContextActions.Add(deleteAction);
		}
	}
}
