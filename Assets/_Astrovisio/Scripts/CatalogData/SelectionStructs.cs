
using System.Collections.Generic;
using UnityEngine;

namespace CatalogData
{
    public enum AreaSelectionMode
    {
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

    public class AreaSelectionResult
    {
        public List<int> SelectedIndices { get; set; }
        public float[] AggregatedValues { get; set; }
        public int Count => SelectedIndices?.Count ?? 0;
        public Vector3 CenterPoint { get; set; }
        public float SelectionRadius { get; set; }

        public AreaSelectionResult()
        {
            SelectedIndices = new List<int>();
        }
    }
}