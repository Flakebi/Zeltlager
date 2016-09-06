using System;
namespace Zeltlager
{
	public interface IEditable<T>
	{
		void OnStartEditing(T obj);
		void OnFinishEditing(T obj);
	}
}

