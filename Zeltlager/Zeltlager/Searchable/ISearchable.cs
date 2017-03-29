using System.Collections.Generic;

namespace Zeltlager
{
	public interface ISearchable
	{
		string SearchableText { get; }
		string SearchableDetail { get; }
	}

	public class SearchableComparator<T> : IComparer<T> where T : ISearchable
	{
		public int Compare(T x, T y)
		{
			return x.SearchableText.CompareTo(y.SearchableText);
		}
	}
}
