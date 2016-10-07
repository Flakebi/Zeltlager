using System;
namespace Zeltlager.UAM
{
	public interface IEditable<T>
	{
		// stuff that should be done if editing is cancelled
		//void OnCancelEditing();

		// stuff that should be done if editing is finished
		// if null is passed, nothing should be deleted
		void OnSaveEditing(T oldObject);

		// deep cloning method used to save object status before editing (so it can be deleted whlie saving the new one)
		T CloneDeep();
	}
}

