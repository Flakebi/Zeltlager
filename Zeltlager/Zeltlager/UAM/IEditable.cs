using System.Threading.Tasks;

namespace Zeltlager.UAM
{
	public interface IEditable<T>
	{
		/// <summary>
		/// Stuff that should be done if editing is finished,
		/// if null is passed, nothing should be deleted.
		/// </summary>
		/// <param name="oldObject">
		/// The old object that was edited.
		/// null if this object is newly created and not edited.
		/// </param>
		Task OnSaveEditing(T oldObject);

		/// <summary>
		/// Deep cloning method used to save object status before editing
		/// (so it can be deleted while saving the new one).
		/// </summary>
		/// <returns>A copy of this object.</returns>
		T CloneDeep();
	}
}
