using System;
using UnityEngine;

namespace Astrovisio
{
    public class DataContainer
    {
        public DataPack DataPack { private set; get; }
        public Project Project { private set; get; }

        // Axis
        public int XAxisIndex { private set; get; }
        public int YAxisIndex { private set; get; }
        public int ZAxisIndex { private set; get; }
        public string XAxisName { private set; get; }
        public string YAxisName { private set; get; }
        public string ZAxisName { private set; get; }


        // Threshold
        public float XMinThreshold { private set; get; }
        public float XMaxThreshold { private set; get; }
        public float YMinThreshold { private set; get; }
        public float YMaxThreshold { private set; get; }
        public float ZMinThreshold { private set; get; }
        public float ZMaxThreshold { private set; get; }

        // Utils
        public float[][] TransposedData { private set; get; }
        public int PointCount { private set; get; }
        public Vector3 MinPoint { private set; get; }
        public Vector3 MaxPoint { private set; get; }
        public Vector3 Center { private set; get; }

#nullable enable
        public DataContainer(DataPack dataPack, Project? project)
        {
            DataPack = dataPack ?? throw new ArgumentNullException(nameof(dataPack));
            Project = project;

            InitAxis();
            InitMinMax();
            InitCenter();
            InitAxisThreshold();

            TransposedData = Transpose(DataPack.Rows);
        }
#nullable disable

        private void InitAxisThreshold()
        {
            if (Project != null)
                foreach (var param in Project.ConfigProcess.Params)
                {
                    // Debug.Log(param.Key + " " + param.Value);
                    if (param.Value.Selected)
                    {
                        if (param.Value.XAxis)
                        {
                            XMinThreshold = (float)param.Value.ThrMinSel;
                            XMaxThreshold = (float)param.Value.ThrMaxSel;
                        }
                        else if (param.Value.YAxis)
                        {
                            YMinThreshold = (float)param.Value.ThrMinSel;
                            YMaxThreshold = (float)param.Value.ThrMaxSel;
                        }
                        else if (param.Value.ZAxis)
                        {
                            ZMinThreshold = (float)param.Value.ThrMinSel;
                            ZMaxThreshold = (float)param.Value.ThrMaxSel;
                        }
                    }
                }
        }

        private void InitAxis()
        {
            if (Project == null)
            {
                return;
            }

            XAxisName = "";
            YAxisName = "";
            ZAxisName = "";

            foreach (var kvp in Project.ConfigProcess.Params)
            {
                string paramName = kvp.Key;
                ConfigParam param = kvp.Value;

                if (param.XAxis)
                {
                    // Debug.Log("x: " + paramName);
                    XAxisName = paramName;
                }
                else if (param.YAxis)
                {
                    // Debug.Log("y: " + paramName);
                    YAxisName = paramName;
                }
                else if (param.ZAxis)
                {
                    // Debug.Log("z: " + paramName);
                    ZAxisName = paramName;
                }
            }
            // Debug.Log("XAxisName: " + XAxisName);
            // Debug.Log("YAxisName: " + YAxisName);
            // Debug.Log("ZAxisName: " + ZAxisName);

            for (int i = 0; i < DataPack.Columns.Length; i++)
            {
                if (DataPack.Columns[i] == XAxisName)
                {
                    XAxisIndex = i;
                }
                else if (DataPack.Columns[i] == YAxisName)
                {
                    YAxisIndex = i;
                }
                else if (DataPack.Columns[i] == ZAxisName)
                {
                    ZAxisIndex = i;
                }
            }
            // Debug.Log("XAxisIndex: " + XAxisIndex);
            // Debug.Log("YAxisIndex: " + YAxisIndex);
            // Debug.Log("ZAxisIndex: " + ZAxisIndex);
        }


        private void InitMinMax()
        {
            if (DataPack.Rows == null || DataPack.Rows.Length == 0)
            {
                throw new InvalidOperationException("No data rows available.");
            }

            double[] firstRow = DataPack.Rows[0];
            Vector3 first = new Vector3(
                (float)firstRow[XAxisIndex],
                (float)firstRow[YAxisIndex],
                (float)firstRow[ZAxisIndex]
            );

            MinPoint = first;
            MaxPoint = first;
            PointCount = 0;

            foreach (double[] row in DataPack.Rows)
            {
                Vector3 point = new Vector3(
                    (float)row[XAxisIndex],
                    (float)row[YAxisIndex],
                    (float)row[ZAxisIndex]
                );

                MinPoint = Vector3.Min(MinPoint, point);
                MaxPoint = Vector3.Max(MaxPoint, point);
                PointCount++;
            }

            // Debug.Log(MinPoint);
            // Debug.Log(MaxPoint);
            // Debug.Log(PointCount);
        }


        private void InitCenter()
        {
            Center = (MinPoint + MaxPoint) / 2f;
        }

        // private Vector3 ToVector3(double[] row)
        // {
        //     if (row.Length < 3)
        //     {
        //         throw new ArgumentException("Each row must have at least 3 values (x, y, z).");
        //     }

        //     return new Vector3((float)row[0], (float)row[1], (float)row[2]);
        // }

        public Vector3[] GetProportionalScaledData(float cubeDim)
        {
            Vector3[] scaled = new Vector3[PointCount];
            Vector3 center = Center;
            Vector3 extents = MaxPoint - MinPoint;

            float maxExtent = Mathf.Max(extents.x, extents.y, extents.z);
            float scale = cubeDim / Mathf.Max(maxExtent, 1e-6f);

            string xAxisName = "";
            string yAxisName = "";
            string zAxisName = "";
            if (Project != null)
                foreach (var kvp in Project.ConfigProcess.Params)
                {
                    string paramName = kvp.Key;
                    ConfigParam param = kvp.Value;

                    // Debug.Log($"Parametro: {paramName}");
                    // Debug.Log($"Valore: {param.XAxis}");

                    if (param.XAxis)
                    {
                        xAxisName = paramName;
                    }
                    else if (param.YAxis)
                    {
                        yAxisName = paramName;
                    }
                    else if (param.ZAxis)
                    {
                        zAxisName = paramName;
                    }
                }
            // Debug.Log("xAxisName: " + xAxisName);
            // Debug.Log("yAxisName: " + yAxisName);
            // Debug.Log("zAxisName: " + zAxisName);

            int xAxis = 0;
            int yAxis = 0;
            int zAxis = 0;
            for (int i = 0; i < DataPack.Columns.Length; i++)
            {
                if (DataPack.Columns[i] == xAxisName)
                {
                    xAxis = i;
                }
                else if (DataPack.Columns[i] == yAxisName)
                {
                    yAxis = i;
                }
                else if (DataPack.Columns[i] == zAxisName)
                {
                    zAxis = i;
                }
            }
            // Debug.Log("xAxis: " + xAxis);
            // Debug.Log("yAxis: " + yAxis);
            // Debug.Log("zAxis: " + zAxis);


            for (int i = 0; i < PointCount; i++)
            {
                float x = (float)DataPack.Rows[i][xAxis];
                float y = (float)DataPack.Rows[i][yAxis];
                float z = (float)DataPack.Rows[i][zAxis];

                Vector3 p = new Vector3(x, y, z);
                Vector3 centered = p - center;
                scaled[i] = centered * scale;
            }
            return scaled;
        }

        private float[][] Transpose(double[][] matrix)
        {
            if (matrix == null || matrix.Length == 0)
                return new float[0][];

            int rowCount = matrix.Length;
            int colCount = matrix[0].Length;

            float[][] transposed = new float[colCount][];

            for (int i = 0; i < colCount; i++)
            {
                transposed[i] = new float[rowCount];
                for (int j = 0; j < rowCount; j++)
                {
                    transposed[i][j] = (float)matrix[j][i];
                }
            }

            return transposed;
        }

    }

}
