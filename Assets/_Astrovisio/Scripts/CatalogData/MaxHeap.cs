using System;
using System.Collections.Generic;

public class MaxHeap<T>
{
    private readonly List<(T item, float priority)> _items = new();
    private readonly IComparer<float> _comparer;

    public MaxHeap()
    {
        _comparer = Comparer<float>.Default;
    }

    public int Count => _items.Count;

    public void Enqueue(T item, float priority)
    {
        _items.Add((item, priority));
        HeapifyUp(_items.Count - 1);
    }

    public T Peek()
    {
        if (_items.Count == 0) throw new InvalidOperationException("Heap is empty");
        return _items[0].item;
    }

    public float PeekPriority()
    {
        if (_items.Count == 0) throw new InvalidOperationException("Heap is empty");
        return _items[0].priority;
    }

    public T Dequeue()
    {
        if (_items.Count == 0) throw new InvalidOperationException("Heap is empty");

        T rootItem = _items[0].item;
        int last = _items.Count - 1;
        _items[0] = _items[last];
        _items.RemoveAt(last);
        HeapifyDown(0);

        return rootItem;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (_comparer.Compare(_items[index].priority, _items[parent].priority) <= 0)
                break;

            Swap(index, parent);
            index = parent;
        }
    }

    private void HeapifyDown(int index)
    {
        int last = _items.Count - 1;
        while (true)
        {
            int largest = index;
            int left = 2 * index + 1;
            int right = 2 * index + 2;

            if (left <= last && _comparer.Compare(_items[left].priority, _items[largest].priority) > 0)
                largest = left;

            if (right <= last && _comparer.Compare(_items[right].priority, _items[largest].priority) > 0)
                largest = right;

            if (largest == index)
                break;

            Swap(index, largest);
            index = largest;
        }
    }

    private void Swap(int i, int j)
    {
        var tmp = _items[i];
        _items[i] = _items[j];
        _items[j] = tmp;
    }

    public List<T> ToSortedList()
    {
        var sorted = new List<T>(_items.Count);
        var copy = new MaxHeap<T>();
        foreach (var (item, priority) in _items)
            copy.Enqueue(item, priority);

        while (copy.Count > 0)
            sorted.Add(copy.Dequeue());

        return sorted;
    }
}