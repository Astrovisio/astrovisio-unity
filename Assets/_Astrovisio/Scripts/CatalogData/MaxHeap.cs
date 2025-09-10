using System.Collections.Generic;

public class MaxHeap<T>
{
    private readonly List<(T item, float priority)> heap = new();

    public int Count => heap.Count;

    public void Enqueue(T item, float priority)
    {
        heap.Add((item, priority));
        HeapifyUp(heap.Count - 1);
    }

    public T Dequeue()
    {
        var root = heap[0].item;
        heap[0] = heap[^1];
        heap.RemoveAt(heap.Count - 1);
        HeapifyDown(0);
        return root;
    }

    public float PeekPriority() => heap[0].priority;

    private void HeapifyUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (heap[i].priority <= heap[parent].priority) break;
            (heap[i], heap[parent]) = (heap[parent], heap[i]);
            i = parent;
        }
    }

    private void HeapifyDown(int i)
    {
        int last = heap.Count - 1;
        while (true)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            int largest = i;

            if (left <= last && heap[left].priority > heap[largest].priority) largest = left;
            if (right <= last && heap[right].priority > heap[largest].priority) largest = right;
            if (largest == i) break;
            (heap[i], heap[largest]) = (heap[largest], heap[i]);
            i = largest;
        }
    }
}
