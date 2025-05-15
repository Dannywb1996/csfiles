using System.Collections;
using System.Collections.Generic;

public class FlatTupleListWrapper<T> : IReadOnlyList<(T, T)>, IEnumerable<(T, T)>, IEnumerable, IReadOnlyCollection<(T, T)>
{
	public IList<T> list;

	public int Count => list.Count / 2;

	public (T, T) this[int index] => (list[index * 2], list[index * 2 + 1]);

	public IEnumerator<(T, T)> GetEnumerator()
	{
		for (int i = 0; i < list.Count / 2; i++)
		{
			yield return (list[i * 2], list[i * 2 + 1]);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
