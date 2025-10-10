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

using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

public class KDTreeManager
{
    private KDTree[] trees = new KDTree[8];
    private Vector3 pivot;
    private float[][] data;
    private int[] xyz;
    private int[] visibilityArray;

    public KDTreeManager(float[][] data, Vector3 pivot, int[] xyz, int[] visibilityArray)
    {
        this.data = data;
        this.pivot = pivot;
        this.xyz = xyz;
        this.visibilityArray = visibilityArray;
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
            if (data[xyz[0]][i] < pivot.x) idx |= 1;
            if (data[xyz[1]][i] < pivot.y) idx |= 2;
            if (data[xyz[2]][i] < pivot.z) idx |= 4;
            buckets[idx].Add(i);
        }

        Parallel.For(0, 8, i =>
        {
            trees[i] = new KDTree(data, buckets[i], xyz, visibilityArray);
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

    public List<int> FindPointsInEllipsoid(Vector3 center, Vector3 radii)
    {
        var allResults = new HashSet<int>();

        // Check which octants the ellipsoid intersects
        for (int i = 0; i < 8; i++)
        {
            if (EllipsoidIntersectsOctant(center, radii, i))
            {
                var results = trees[i].FindPointsInEllipsoid(center, radii);
                foreach (var idx in results)
                {
                    allResults.Add(idx);
                }
            }
        }

        return allResults.ToList();
    }

    public List<int> FindPointsInBox(Vector3 center, Vector3 halfSizes)
    {
        var allResults = new HashSet<int>();

        // Check which octants the box intersects
        for (int i = 0; i < 8; i++)
        {
            if (BoxIntersectsOctant(center, halfSizes, i))
            {
                var results = trees[i].FindPointsInBox(center, halfSizes);
                foreach (var idx in results)
                {
                    allResults.Add(idx);
                }
            }
        }

        return allResults.ToList();
    }

    private bool EllipsoidIntersectsOctant(Vector3 center, Vector3 radii, int octantIndex)
    {
        // Calculate octant bounds based on pivot
        Vector3 octantMin = new Vector3(
            (octantIndex & 1) != 0 ? float.MinValue : pivot.x,
            (octantIndex & 2) != 0 ? float.MinValue : pivot.y,
            (octantIndex & 4) != 0 ? float.MinValue : pivot.z
        );

        Vector3 octantMax = new Vector3(
            (octantIndex & 1) != 0 ? pivot.x : float.MaxValue,
            (octantIndex & 2) != 0 ? pivot.y : float.MaxValue,
            (octantIndex & 4) != 0 ? pivot.z : float.MaxValue
        );

        // Check ellipsoid-AABB intersection
        Vector3 closest = Vector3.Max(octantMin, Vector3.Min(center, octantMax));
        Vector3 distance = closest - center;

        // Normalize by radii for ellipsoid test
        float normalizedDistSq = (distance.x * distance.x) / (radii.x * radii.x) +
                                (distance.y * distance.y) / (radii.y * radii.y) +
                                (distance.z * distance.z) / (radii.z * radii.z);

        return normalizedDistSq <= 1.0f;
    }

    private bool BoxIntersectsOctant(Vector3 center, Vector3 halfSizes, int octantIndex)
    {
        Vector3 boxMin = center - halfSizes;
        Vector3 boxMax = center + halfSizes;

        // Calculate octant bounds
        Vector3 octantMin = new Vector3(
            (octantIndex & 1) != 0 ? float.MinValue : pivot.x,
            (octantIndex & 2) != 0 ? float.MinValue : pivot.y,
            (octantIndex & 4) != 0 ? float.MinValue : pivot.z
        );

        Vector3 octantMax = new Vector3(
            (octantIndex & 1) != 0 ? pivot.x : float.MaxValue,
            (octantIndex & 2) != 0 ? pivot.y : float.MaxValue,
            (octantIndex & 4) != 0 ? pivot.z : float.MaxValue
        );

        // Check AABB-AABB intersection
        return boxMin.x <= octantMax.x && boxMax.x >= octantMin.x &&
               boxMin.y <= octantMax.y && boxMax.y >= octantMin.y &&
               boxMin.z <= octantMax.z && boxMax.z >= octantMin.z;
    }

    private bool SphereIntersectsOctant(Vector3 center, float radius, int octantIndex)
    {
        // Calculate octant bounds based on pivot
        Vector3 octantMin = new Vector3(
            (octantIndex & 1) != 0 ? float.MinValue : pivot.x,
            (octantIndex & 2) != 0 ? float.MinValue : pivot.y,
            (octantIndex & 4) != 0 ? float.MinValue : pivot.z
        );

        Vector3 octantMax = new Vector3(
            (octantIndex & 1) != 0 ? pivot.x : float.MaxValue,
            (octantIndex & 2) != 0 ? pivot.y : float.MaxValue,
            (octantIndex & 4) != 0 ? pivot.z : float.MaxValue
        );

        // Check sphere-AABB intersection
        Vector3 closest = Vector3.Max(octantMin, Vector3.Min(center, octantMax));
        return (closest - center).sqrMagnitude <= radius * radius;
    }

    private bool CubeIntersectsOctant(Vector3 center, float halfSize, int octantIndex)
    {
        Vector3 cubeMin = center - Vector3.one * halfSize;
        Vector3 cubeMax = center + Vector3.one * halfSize;

        // Calculate octant bounds
        Vector3 octantMin = new Vector3(
            (octantIndex & 1) != 0 ? float.MinValue : pivot.x,
            (octantIndex & 2) != 0 ? float.MinValue : pivot.y,
            (octantIndex & 4) != 0 ? float.MinValue : pivot.z
        );

        Vector3 octantMax = new Vector3(
            (octantIndex & 1) != 0 ? pivot.x : float.MaxValue,
            (octantIndex & 2) != 0 ? pivot.y : float.MaxValue,
            (octantIndex & 4) != 0 ? pivot.z : float.MaxValue
        );

        // Check AABB-AABB intersection
        return cubeMin.x <= octantMax.x && cubeMax.x >= octantMin.x &&
               cubeMin.y <= octantMax.y && cubeMax.y >= octantMin.y &&
               cubeMin.z <= octantMax.z && cubeMax.z >= octantMin.z;
    }

}