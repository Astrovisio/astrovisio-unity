/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Alkemy, Metaverso
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

namespace CatalogData
{
    public enum SelectionMode
    {
        None,
        SinglePoint,
        Sphere,
        Cube
    }

    public enum AggregationMode
    {
        Average,
        Sum,
        Min,
        Max,
        Median
    }

    public class SelectionResult
    {
        public HashSet<int> SelectedIndices { get; set; }
        public int[] SelectedArray { get; set; }
        public float[] AggregatedValues { get; set; }
        public int Count => SelectedIndices?.Count ?? 0;
        public Vector3 CenterPoint { get; set; }
        public float SelectionRadius { get; set; }
        public SelectionMode SelectionMode { get;  set; }

        public SelectionResult()
        {
            SelectedIndices = new HashSet<int>();
        }
    }
}