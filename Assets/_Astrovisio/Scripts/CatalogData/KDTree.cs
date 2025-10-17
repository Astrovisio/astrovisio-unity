/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Metaverso SRL
 *
 * This file is part of the Astrovisio project.
 *
 * Astrovisio is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Astrovisio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class KDTree
{
    private readonly float[][] data;
    private readonly int[] indices;
    private KDTreeNode root;
    private int[] xyz;
    private int[] visibilityArray;
    private CancellationToken cancellationToken;

    private class KDTreeNode
    {
        public int index;
        public Vector3 point;
        public KDTreeNode left;
        public KDTreeNode right;
    }

    public KDTree(float[][] data, int[] pointIndices, int[] xyz, int[] visibilityArray, CancellationToken token = default)
    {
        this.data = data;
        this.xyz = xyz;
        this.visibilityArray = visibilityArray;
        this.cancellationToken = token;
        indices = pointIndices;
        root = BuildTree(0, indices.Length, 0);
    }

    private KDTreeNode BuildTree(int start, int end, int depth)
    {
        if (start >= end) return null;

        if (((start + depth) & 0x3FF) == 0)
            cancellationToken.ThrowIfCancellationRequested();

        int axis = this.xyz[depth % 3];
        Array.Sort(indices, start, end - start, Comparer<int>.Create((a, b) =>
            data[axis][a].CompareTo(data[axis][b])
        ));

        int mid = (start + end) / 2;
        return new KDTreeNode
        {
            index = indices[mid],
            point = new Vector3(data[xyz[0]][indices[mid]], data[xyz[1]][indices[mid]], data[xyz[2]][indices[mid]]),
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

        int depthMod = depth % 3;
        int axis = xyz[depthMod];
        float targetVal = depthMod == 0 ? target.x : depthMod == 1 ? target.y : target.z;
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
        float distSq = (node.point - target).sqrMagnitude;

        if (heap.Count < k)
            heap.Enqueue((idx, distSq), distSq);
        else if (distSq < heap.PeekPriority())
        {
            heap.Dequeue();
            heap.Enqueue((idx, distSq), distSq);
        }

        int depthMod = depth % 3;
        int axis = this.xyz[depthMod];
        float targetVal = depthMod == 0 ? target.x : depthMod == 1 ? target.y : target.z;
        float nodeVal = data[axis][idx];

        KDTreeNode near = targetVal < nodeVal ? node.left : node.right;
        KDTreeNode far = targetVal < nodeVal ? node.right : node.left;

        SearchKNearest(near, target, depth + 1, heap, k);
        if ((targetVal - nodeVal) * (targetVal - nodeVal) < (heap.Count < k ? float.MaxValue : heap.PeekPriority()))
            SearchKNearest(far, target, depth + 1, heap, k);
    }


    public HashSet<int> FindPointsInSphere(Vector3 center, float radius)
    {
        var result = new HashSet<int>();
        float radiusSq = radius * radius;
        SearchSphere(root, center, radiusSq, 0, result);
        return result;
    }

    private void SearchSphere(KDTreeNode node, Vector3 center, float radiusSq, int depth, HashSet<int> result)
    {
        if (node == null) return;

        float distSq = (node.point - center).sqrMagnitude;
        if (distSq <= radiusSq)
        {
            result.Add(node.index);
            visibilityArray[node.index] = 1;
        }

        int depthMod = depth % 3;
        int axis = xyz[depthMod];
        float centerVal = depthMod == 0 ? center.x : depthMod == 1 ? center.y : center.z;
        float nodeVal = data[axis][node.index];

        // Calculate distance from center to splitting plane
        float planeDist = centerVal - nodeVal;
        float planeDistSq = planeDist * planeDist;

        // Visit near side
        KDTreeNode near = planeDist < 0 ? node.left : node.right;
        SearchSphere(near, center, radiusSq, depth + 1, result);

        // Check if we need to visit far side
        if (planeDistSq <= radiusSq)
        {
            KDTreeNode far = planeDist < 0 ? node.right : node.left;
            SearchSphere(far, center, radiusSq, depth + 1, result);
        }
    }

    public HashSet<int> FindPointsInCube(Vector3 center, float halfSize)
    {
        var result = new HashSet<int>();
        Vector3 min = center - Vector3.one * halfSize;
        Vector3 max = center + Vector3.one * halfSize;
        SearchCube(root, min, max, 0, result);
        return result;
    }

    private void SearchCube(KDTreeNode node, Vector3 min, Vector3 max, int depth, HashSet<int> result)
    {
        if (node == null) return;

        // Check if point is within cube
        if (node.point.x >= min.x && node.point.x <= max.x &&
            node.point.y >= min.y && node.point.y <= max.y &&
            node.point.z >= min.z && node.point.z <= max.z)
        {
            result.Add(node.index);
            visibilityArray[node.index] = 1;
        }

        int depthMod = depth % 3;
        int axis = xyz[depthMod];
        float nodeVal = data[axis][node.index];
        float minVal = depthMod == 0 ? min.x : depthMod == 1 ? min.y : min.z;
        float maxVal = depthMod == 0 ? max.x : depthMod == 1 ? max.y : max.z;

        // Visit children if their regions overlap with the cube
        if (minVal <= nodeVal)
            SearchCube(node.left, min, max, depth + 1, result);

        if (maxVal >= nodeVal)
            SearchCube(node.right, min, max, depth + 1, result);
    }

    public HashSet<int> FindPointsInEllipsoid(Vector3 center, Vector3 radii)
    {
        var result = new HashSet<int>();
        SearchEllipsoid(root, center, radii, 0, result);
        return result;
    }

    private void SearchEllipsoid(KDTreeNode node, Vector3 center, Vector3 radii, int depth, HashSet<int> result)
    {
        if (node == null) return;

        // Check if point is within ellipsoid
        Vector3 distance = node.point - center;
        float normalizedDistSq = (distance.x * distance.x) / (radii.x * radii.x) +
                                (distance.y * distance.y) / (radii.y * radii.y) +
                                (distance.z * distance.z) / (radii.z * radii.z);

        if (normalizedDistSq <= 1.0f)
        {
            result.Add(node.index);
            visibilityArray[node.index] = 1;
        }

        int depthMod = depth % 3;
        int axis = xyz[depthMod];
        float centerVal = depthMod == 0 ? center.x : depthMod == 1 ? center.y : center.z;
        float radiusVal = depthMod == 0 ? radii.x : depthMod == 1 ? radii.y : radii.z;
        float nodeVal = data[axis][node.index];

        // Calculate distance from center to splitting plane
        float planeDist = centerVal - nodeVal;

        // Visit near side
        KDTreeNode near = planeDist < 0 ? node.left : node.right;
        SearchEllipsoid(near, center, radii, depth + 1, result);

        // Check if we need to visit far side (using appropriate radius for this axis)
        if (Mathf.Abs(planeDist) <= radiusVal)
        {
            KDTreeNode far = planeDist < 0 ? node.right : node.left;
            SearchEllipsoid(far, center, radii, depth + 1, result);
        }
    }

    public HashSet<int> FindPointsInBox(Vector3 center, Vector3 halfSizes)
    {
        var result = new HashSet<int>();
        Vector3 min = center - halfSizes;
        Vector3 max = center + halfSizes;
        SearchBox(root, min, max, 0, result);
        return result;
    }

    private void SearchBox(KDTreeNode node, Vector3 min, Vector3 max, int depth, HashSet<int> result)
    {
        if (node == null) return;

        // Check if point is within box
        if (node.point.x >= min.x && node.point.x <= max.x &&
            node.point.y >= min.y && node.point.y <= max.y &&
            node.point.z >= min.z && node.point.z <= max.z)
        {
            result.Add(node.index);
            visibilityArray[node.index] = 1;
        }

        int depthMod = depth % 3;
        int axis = xyz[depthMod];
        float nodeVal = data[axis][node.index];
        float minVal = depthMod == 0 ? min.x : depthMod == 1 ? min.y : min.z;
        float maxVal = depthMod == 0 ? max.x : depthMod == 1 ? max.y : max.z;

        // Visit children if their regions overlap with the box
        if (minVal <= nodeVal)
            SearchBox(node.left, min, max, depth + 1, result);

        if (maxVal >= nodeVal)
            SearchBox(node.right, min, max, depth + 1, result);
    }

}