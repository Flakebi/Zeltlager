using System.ComponentModel;
using Xamarin.Forms;

namespace Zeltlager
{
	public class ActionCell : ViewCell
	{
		/// <summary>
		/// The bindable property implementation
		/// </summary>
		public static readonly BindableProperty OnEditCommandParameterProperty = BindableProperty.Create(nameof(OnEditCommandParameter), typeof(object), typeof(ActionCell), null);
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
		public static readonly BindableProperty OnEditCommandProperty = BindableProperty.Create(nameof(OnEditCommand), typeof(Command), typeof(ActionCell), null);
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

		public static readonly BindableProperty OnDeleteCommandParameterProperty = BindableProperty.Create(nameof(OnDeleteCommandParameter), typeof(object), typeof(ActionCell), null);
		/// <summary>
		/// The delete command parameter
		/// </summary>
		public object OnDeleteCommandParameter
		{
			get { return GetValue(OnDeleteCommandParameterProperty); }
			set
			{
				SetValue(OnDeleteCommandParameterProperty, value);
				OnPropertyChanged();
			}
		}
		/// <summary>
		/// The bindable property implementation
		/// </summary>
		public static readonly BindableProperty OnDeleteCommandProperty = BindableProperty.Create(nameof(OnDeleteCommand), typeof(Command), typeof(ActionCell), null);
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

		MenuItem editAction, deleteAction;

		public ActionCell()
		{
			editAction = new MenuItem { Icon = Icons.EDIT, Text = Icons.EDIT_TEXT };
			editAction.SetBinding(MenuItem.CommandParameterProperty, new Binding(nameof(OnEditCommandParameter)));
			editAction.SetBinding(MenuItem.CommandProperty, new Binding(nameof(OnEditCommand)));
			editAction.BindingContext = this;

			deleteAction = new MenuItem { Icon = Icons.DELETE, Text = Icons.DELETE_TEXT, IsDestructive = true };
			deleteAction.SetBinding(MenuItem.CommandParameterProperty, new Binding(nameof(OnDeleteCommandParameter)));
			deleteAction.SetBinding(MenuItem.CommandProperty, new Binding(nameof(OnDeleteCommand)));
			deleteAction.BindingContext = this;

			PropertyChanged += PropertyChangedEvent;
		}

		void PropertyChangedEvent(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(OnEditCommand))
			{
				if (ContextActions.Contains(editAction) && OnEditCommand == null)
					ContextActions.Remove(editAction);
				if (!ContextActions.Contains(editAction) && OnEditCommand != null)
					ContextActions.Add(editAction);
			}
			else if (e.PropertyName == nameof(OnDeleteCommand))
			{
				if (ContextActions.Contains(deleteAction) && OnDeleteCommand == null)
					ContextActions.Remove(deleteAction);
				if (!ContextActions.Contains(deleteAction) && OnDeleteCommand != null)
					ContextActions.Add(deleteAction);
			}
		}
	}
}
