using System;
using UnityEngine;

namespace Astrovisio
{
    public class DataContainer
    {
        public DataPack DataPack { private set; get; }
        public int PointCount { private set; get; }
        public Vector3 MinPoint { private set; get; }
        public Vector3 MaxPoint { private set; get; }
        public Vector3 Center { private set; get; }

        public DataContainer(DataPack dataPack)
        {
            DataPack = dataPack ?? throw new ArgumentNullException(nameof(dataPack));
            ComputeMinMax();
            ComputeCenter();
        }

        private void ComputeMinMax()
        {
            if (DataPack.Rows == null || DataPack.Rows.Length == 0)
            {
                throw new InvalidOperationException("No data rows available.");
            }

            var first = ToVector3(DataPack.Rows[0]);
            MinPoint = first;
            MaxPoint = first;
            PointCount = 0;

            foreach (var row in DataPack.Rows)
            {
                var point = ToVector3(row);
                MinPoint = Vector3.Min(MinPoint, point);
                MaxPoint = Vector3.Max(MaxPoint, point);
                PointCount++;
            }
        }

        private void ComputeCenter()
        {
            Center = (MinPoint + MaxPoint) / 2f;
        }

        private Vector3 ToVector3(double[] row)
        {
            if (row.Length < 3)
            {
                throw new ArgumentException("Each row must have at least 3 values (x, y, z).");
            }

            return new Vector3((float)row[0], (float)row[1], (float)row[2]);
        }

        public Vector3[] GetProportionalScaledData(float cubeDim)
        {
            Vector3[] scaled = new Vector3[PointCount];
            Vector3 center = Center;
            Vector3 extents = MaxPoint - MinPoint;

            float maxExtent = Mathf.Max(extents.x, extents.y, extents.z);
            float scale = cubeDim / Mathf.Max(maxExtent, 1e-6f);

            for (int i = 0; i < PointCount; i++)
            {
                Vector3 p = ToVector3(DataPack.Rows[i]);
                Vector3 centered = p - center;
                scaled[i] = centered * scale;
            }
            return scaled;
        }

    }
}
