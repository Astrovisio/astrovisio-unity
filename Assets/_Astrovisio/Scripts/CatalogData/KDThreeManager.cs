using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class KDTreeManager
{
    private KDTree[] trees = new KDTree[8];
    private Vector3 pivot;
    private float[][] data;

    public KDTreeManager(float[][] data, Vector3 pivot)
    {
        this.data = data;
        this.pivot = pivot;
        BuildTrees();
    }

    private void BuildTrees()
    {
        List<int>[] buckets = new List<int>[8];
        for (int i = 0; i < 8; i++) buckets[i] = new List<int>();

        int N = data[0].Length;
        for (int i = 0; i < N; i++)
        {
            int idx = 0;
            if (data[0][i] < pivot.x) idx |= 1;
            if (data[1][i] < pivot.y) idx |= 2;
            if (data[2][i] < pivot.z) idx |= 4;
            buckets[idx].Add(i);
        }

        Parallel.For(0, 8, i =>
        {
            trees[i] = new KDTree(data, buckets[i]);
        });
    }

    private int GetOctant(Vector3 point)
    {
        int idx = 0;
        if (point.x < pivot.x) idx |= 1;
        if (point.y < pivot.y) idx |= 2;
        if (point.z < pivot.z) idx |= 4;
        return idx;
    }

    public (int index, float distanceSquared) FindNearest(Vector3 query)
    {
        int octant = GetOctant(query);
        return trees[octant].FindNearest(query);
    }

    public List<(int index, float distanceSquared)> FindKNearest(Vector3 query, int k)
    {
        var heap = new MaxHeap<(int, float)>();
        for (int i = 0; i < 8; i++)
        {
            var results = trees[i].FindKNearest(query, k);
            foreach (var res in results)
            {
                if (heap.Count < k)
                    heap.Enqueue(res, res.Item2);
                else if (res.Item2 < heap.PeekPriority())
                {
                    heap.Dequeue();
                    heap.Enqueue(res, res.Item2);
                }
            }
        }

        var list = new List<(int, float)>(heap.Count);
        while (heap.Count > 0)
            list.Add(heap.Dequeue());

        list.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        return list;
    }
}