using UnityEngine;

namespace Astrovisio
{
    public class DataCubeRenderer : MonoBehaviour
    {
        private ComputeBuffer _xBuffer, _yBuffer, _zBuffer;
        private Material _material;

        private DataContainer dataContainer;
        private Shader pointShader;
        private int _pointCount;

        public void Initialize(DataContainer dataContainer, Shader shader = null)
        {
            this.dataContainer = dataContainer;
            Debug.Log("[DataCubeRenderer] Initialize chiamato con " + this.dataContainer.PointCount + " punti");

            if (dataContainer == null || this.dataContainer.DataPack == null || this.dataContainer.PointCount == 0)
            {
                Debug.LogError("DataCubeRenderer: container non valido.");
                return;
            }

            _pointCount = this.dataContainer.PointCount;
            pointShader = shader ?? Shader.Find("Hidden/SimpleXYZPointShader");

            if (pointShader == null)
            {
                Debug.LogError("DataCubeRenderer: Shader non trovato.");
                return;
            }

            Vector3[] normalizedPoints = this.dataContainer.GetProportionalScaledData(2f);

            float[] x = new float[_pointCount];
            float[] y = new float[_pointCount];
            float[] z = new float[_pointCount];
            for (int i = 0; i < _pointCount; i++)
            {
                x[i] = normalizedPoints[i].x;
                y[i] = normalizedPoints[i].y;
                z[i] = normalizedPoints[i].z;
            }

            _xBuffer = new ComputeBuffer(_pointCount, sizeof(float));
            _yBuffer = new ComputeBuffer(_pointCount, sizeof(float));
            _zBuffer = new ComputeBuffer(_pointCount, sizeof(float));
            _xBuffer.SetData(x);
            _yBuffer.SetData(y);
            _zBuffer.SetData(z);

            _material = new Material(pointShader);
            _material.SetBuffer("dataX", _xBuffer);
            _material.SetBuffer("dataY", _yBuffer);
            _material.SetBuffer("dataZ", _zBuffer);
        }

        private void OnRenderObject()
        {
            if (_material == null || _pointCount <= 0)
            {
                return;
            }

            _material.SetMatrix("datasetMatrix", transform.localToWorldMatrix);
            _material.SetFloat("scalingFactor", transform.localScale.x);
            _material.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Points, _pointCount);
        }

        private void ReleaseBuffers()
        {
            _xBuffer?.Release();
            _yBuffer?.Release();
            _zBuffer?.Release();
            _xBuffer = _yBuffer = _zBuffer = null;
        }

        private void OnDisable() => ReleaseBuffers();
        private void OnDestroy() => ReleaseBuffers();

    }
}
