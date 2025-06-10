using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KDTree
{
    private float[][] data;   // float[4][N]
    private int[] indices;    // Indici dei punti
    private KDTreeNode root;

    public KDTree(float[][] data)
    {
        this.data = data;
        int count = data[0].Length;
        this.indices = Enumerable.Range(0, count).ToArray();
        this.root = BuildTree(0, count, 0);
    }

    private class KDTreeNode
    {
        public int index;
        public int axis;
        public KDTreeNode left;
        public KDTreeNode right;
        public Vector3 point;
    }

    private KDTreeNode BuildTree(int start, int end, int depth)
    {
        if (start >= end)
            return null;

        int axis = depth % 3;
        int mid = (start + end) / 2;

        Array.Sort(indices, start, end - start, Comparer<int>.Create((a, b) =>
            data[axis][a].CompareTo(data[axis][b])
        ));

        var node = new KDTreeNode
        {
            index = indices[mid],
            axis = axis,
            point = new Vector3(data[0][indices[mid]], data[1][indices[mid]], data[2][indices[mid]]),
            left = BuildTree(start, mid, depth + 1),
            right = BuildTree(mid + 1, end, depth + 1)
        };

        return node;
    }

    public (int index, float distanceSquared) FindNearest(Vector3 target)
    {
        float bestDist = float.MaxValue;
        int bestIndex = -1;
        Search(root, target, ref bestDist, ref bestIndex);
        return (bestIndex, bestDist);
    }

    public List<(int index, float distanceSquared)> FindKNearest(Vector3 target, int k)
    {
        var heap = new MaxHeap<(int index, float distanceSquared)>();
        SearchKNearest(root, target, 0, k, heap);

        var result = new List<(int, float)>(heap.Count);
        while (heap.Count > 0)
            result.Add(heap.Dequeue());

        // Ordinamento dal più vicino al più lontano
        result.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        return result;
    }

    private void Search(KDTreeNode node, Vector3 target, ref float bestDist, ref int bestIndex)
    {
        if (node == null) return;

        int i = node.index;
        Vector3 point = new Vector3(data[0][i], data[1][i], data[2][i]);
        float dist = (target - point).sqrMagnitude;

        if (dist < bestDist)
        {
            bestDist = dist;
            bestIndex = i;
        }

        int axis = node.axis;
        float delta = GetComponent(target, axis) - data[axis][i];

        KDTreeNode first = delta < 0 ? node.left : node.right;
        KDTreeNode second = delta < 0 ? node.right : node.left;

        Search(first, target, ref bestDist, ref bestIndex);

        if (delta * delta < bestDist)
            Search(second, target, ref bestDist, ref bestIndex);
    }

    private void SearchKNearest(KDTreeNode node, Vector3 target, int depth, int k, MaxHeap<(int, float)> heap)
    {
        if (node == null) return;

        int axis = depth % 3;
        float targetVal = axis == 0 ? target.x : (axis == 1 ? target.y : target.z);
        float nodeVal = axis == 0 ? node.point.x : (axis == 1 ? node.point.y : node.point.z);

        KDTreeNode near = targetVal < nodeVal ? node.left : node.right;
        KDTreeNode far = targetVal < nodeVal ? node.right : node.left;

        // Visita ramo vicino
        SearchKNearest(near, target, depth + 1, k, heap);

        float distSqr = (target - node.point).sqrMagnitude;

        if (heap.Count < k)
        {
            heap.Enqueue((node.index, distSqr), distSqr);
        }
        else if (distSqr < heap.PeekPriority())
        {
            heap.Dequeue();
            heap.Enqueue((node.index, distSqr), distSqr);
        }

        float delta = targetVal - nodeVal;
        float deltaSqr = delta * delta;

        if (heap.Count < k || deltaSqr < heap.PeekPriority())
        {
            SearchKNearest(far, target, depth + 1, k, heap);
        }
    }

    private float GetComponent(Vector3 v, int axis)
    {
        return axis switch
        {
            0 => v.x,
            1 => v.y,
            2 => v.z,
            _ => throw new Exception("Asse non valido")
        };
    }
}