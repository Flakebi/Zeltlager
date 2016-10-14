using System;
using Xamarin.Forms;
namespace Zeltlager
{
	public class SearchableCell : TextCell
	{
		public SearchableCell()
		{
			var editAction = new MenuItem { Text = Icons.EDIT };
			editAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
			editAction.SetBinding(MenuItem.CommandProperty, new Binding("OnEditCommand"));

			var deleteAction = new MenuItem { Text = Icons.DELETE, IsDestructive = true };
			deleteAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
			deleteAction.SetBinding(MenuItem.CommandProperty, new Binding("OnDeleteCommand"));

			ContextActions.Add(editAction);
			ContextActions.Add(deleteAction);
		}

		/// <summary>
		/// The bindable property implementation
		/// </summary>
		public static readonly BindableProperty OnEditCommandParameterProperty = BindableProperty.Create("OnEditCommandParameter", typeof(object), typeof(SearchableCell), null);
		/// <summary>
		/// The command parameter
		/// </summary>
		public object OnEditCommandParameter
		{
			get
			{
				return this.GetValue(OnEditCommandParameterProperty);
			}
			set
			{
				this.SetValue(OnEditCommandParameterProperty, value);
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
			get
			{
				return (Command)this.GetValue(OnEditCommandProperty);
			}
			set
			{
				this.SetValue(OnEditCommandProperty, value);
			}
		}

		public static readonly BindableProperty OnDeleteCommandParameterProperty = BindableProperty.Create("OnDeleteCommandParameter", typeof(object), typeof(SearchableCell), null);
		/// <summary>
		/// The command parameter
		/// </summary>
		public object OnDeleteCommandParameter
		{
			get
			{
				return this.GetValue(OnDeleteCommandParameterProperty);
			}
			set
			{
				this.SetValue(OnEditCommandParameterProperty, value);
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
			get
			{
				return (Command)this.GetValue(OnDeleteCommandProperty);
			}
			set
			{
				this.SetValue(OnDeleteCommandProperty, value);
			}
		}
	}
}
