using System;
using System.Collections.Generic;
using UnityEngine;

public class KDTree
{
    private readonly float[][] data;
    private readonly int[] indices;
    private KDTreeNode root;

    private class KDTreeNode
    {
        public int index;
        public Vector3 point;
        public KDTreeNode left;
        public KDTreeNode right;
    }

    public KDTree(float[][] data, List<int> pointIndices)
    {
        this.data = data;
        this.indices = pointIndices.ToArray();
        root = BuildTree(0, indices.Length, 0);
    }

    private KDTreeNode BuildTree(int start, int end, int depth)
    {
        if (start >= end) return null;

        int axis = depth % 3;
        Array.Sort(indices, start, end - start, Comparer<int>.Create((a, b) =>
            data[axis][a].CompareTo(data[axis][b])
        ));

        int mid = (start + end) / 2;
        return new KDTreeNode
        {
            index = indices[mid],
            point = new Vector3(data[0][indices[mid]], data[1][indices[mid]], data[2][indices[mid]]),
            left = BuildTree(start, mid, depth + 1),
            right = BuildTree(mid + 1, end, depth + 1)
        };
    }

    public (int index, float distanceSquared) FindNearest(Vector3 target)
    {
        return SearchNearest(root, target, 0, (-1, float.MaxValue));
    }

    private (int, float) SearchNearest(KDTreeNode node, Vector3 target, int depth, (int, float) best)
    {
        if (node == null) return best;

        int idx = node.index;
        float distSq = (node.point - target).sqrMagnitude;

        if (distSq < best.Item2) best = (idx, distSq);

        int axis = depth % 3;
        float targetVal = axis == 0 ? target.x : axis == 1 ? target.y : target.z;
        float nodeVal = data[axis][idx];

        KDTreeNode near = targetVal < nodeVal ? node.left : node.right;
        KDTreeNode far = targetVal < nodeVal ? node.right : node.left;

        best = SearchNearest(near, target, depth + 1, best);
        if ((targetVal - nodeVal) * (targetVal - nodeVal) < best.Item2)
            best = SearchNearest(far, target, depth + 1, best);

        return best;
    }

    public List<(int, float)> FindKNearest(Vector3 target, int k)
    {
        var heap = new MaxHeap<(int, float)>();
        SearchKNearest(root, target, 0, heap, k);

        var result = new List<(int, float)>(heap.Count);
        while (heap.Count > 0)
            result.Add(heap.Dequeue());

        result.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        return result;
    }

    private void SearchKNearest(KDTreeNode node, Vector3 target, int depth, MaxHeap<(int, float)> heap, int k)
    {
        if (node == null) return;

        int idx = node.index;
        Vector3 point = new Vector3(data[0][idx], data[1][idx], data[2][idx]);
        float distSq = (point - target).sqrMagnitude;

        if (heap.Count < k)
            heap.Enqueue((idx, distSq), distSq);
        else if (distSq < heap.PeekPriority())
        {
            heap.Dequeue();
            heap.Enqueue((idx, distSq), distSq);
        }

        int axis = depth % 3;
        float targetVal = axis == 0 ? target.x : axis == 1 ? target.y : target.z;
        float nodeVal = data[axis][idx];

        KDTreeNode near = targetVal < nodeVal ? node.left : node.right;
        KDTreeNode far = targetVal < nodeVal ? node.right : node.left;

        SearchKNearest(near, target, depth + 1, heap, k);
        if ((targetVal - nodeVal) * (targetVal - nodeVal) < (heap.Count < k ? float.MaxValue : heap.PeekPriority()))
            SearchKNearest(far, target, depth + 1, heap, k);
    }
}