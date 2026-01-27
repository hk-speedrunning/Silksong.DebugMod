using System.Collections;
using System.Collections.Generic;

namespace DebugMod.Helpers;

internal class LinkedHashSet<T> : ICollection<T>
{
    private readonly LinkedList<T> list = [];
    private readonly Dictionary<T, LinkedListNode<T>> dict = [];

    public int Count => list.Count;

    public bool IsReadOnly => false;

    public bool Contains(T item) => dict.ContainsKey(item);

    public void Clear()
    {
        list.Clear();
        dict.Clear();
    }

    public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

    public bool Add(T item)
    {
        if (dict.ContainsKey(item)) return false;

        dict.Add(item, list.AddLast(item));
        return true;
    }

    void ICollection<T>.Add(T item) => Add(item);

    public bool Remove(T item)
    {
        if (!dict.TryGetValue(item, out var node)) return false;

        list.Remove(node);
        dict.Remove(item);
        return true;
    }

    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
}
