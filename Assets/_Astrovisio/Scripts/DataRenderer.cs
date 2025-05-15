using System;
using UnityEngine;

namespace Astrovisio
{
    public class DataRenderer : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ProjectManager projectManager;

        [Header("Shader")]
        [SerializeField] private Shader pointShader;

        private bool _dataReady = false;
        private int _pointCount;
        private Material _material;
        private ComputeBuffer _bufferX, _bufferY, _bufferZ;

        // Bounding info
        private Vector3 _boundsCenter;
        private Vector3 _boundsSize;

        private void OnEnable()
        {
            projectManager.ProjectProcessed += OnProjectProcessed;
        }

        private void OnDisable()
        {
            projectManager.ProjectProcessed -= OnProjectProcessed;
            ReleaseBuffers();
        }

        private void OnProjectProcessed(ProcessedData data)
        {
            LoadFromProcessedData(data);
        }

        private void LoadFromProcessedData(ProcessedData data)
        {
            // Calcola punto e bounding box
            _pointCount = data.Rows.Length;
            if (_pointCount == 0) return;

            var xs = new float[_pointCount];
            var ys = new float[_pointCount];
            var zs = new float[_pointCount];

            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            for (int i = 0; i < _pointCount; i++)
            {
                var row = data.Rows[i];
                float x = row.Length > 0 ? (float)row[0] : 0f;
                float y = row.Length > 1 ? (float)row[1] : 0f;
                float z = row.Length > 2 ? (float)row[2] : 0f;

                xs[i] = x;
                ys[i] = y;
                zs[i] = z;

                minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                minZ = Mathf.Min(minZ, z); maxZ = Mathf.Max(maxZ, z);
            }

            // Centro e size del bounding box in world coords
            _boundsCenter = new Vector3(
                (minX + maxX) * 0.5f,
                (minY + maxY) * 0.5f,
                (minZ + maxZ) * 0.5f
            );

            _boundsSize = new Vector3(
                maxX - minX,
                maxY - minY,
                maxZ - minZ
            );

            // Sposta il GameObject al centro del bounding box
            transform.position = _boundsCenter;

            // Ora calcola le coordinate locali, sottraendo _boundsCenter
            for (int i = 0; i < _pointCount; i++)
            {
                xs[i] = xs[i] - _boundsCenter.x;
                ys[i] = ys[i] - _boundsCenter.y;
                zs[i] = zs[i] - _boundsCenter.z;
            }

            // Ricrea i buffer
            ReleaseBuffers();
            _bufferX = new ComputeBuffer(_pointCount, sizeof(float));
            _bufferY = new ComputeBuffer(_pointCount, sizeof(float));
            _bufferZ = new ComputeBuffer(_pointCount, sizeof(float));
            _bufferX.SetData(xs);
            _bufferY.SetData(ys);
            _bufferZ.SetData(zs);

            // Materiale
            if (pointShader == null)
                pointShader = Shader.Find("Hidden/SimpleXYZPointShader");

            _material = new Material(pointShader);
            _material.SetBuffer("dataX", _bufferX);
            _material.SetBuffer("dataY", _bufferY);
            _material.SetBuffer("dataZ", _bufferZ);

            gameObject.transform.position = new Vector3(0, 0, 0);
            gameObject.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            Debug.Log(gameObject.transform.localScale);

            _dataReady = true;
        }

        private void OnRenderObject()
        {
            if (!_dataReady || _material == null) return;

            _material.SetMatrix("datasetMatrix", transform.localToWorldMatrix);
            _material.SetFloat("scalingFactor", transform.localScale.x);

            _material.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Points, _pointCount);
        }

        private void ReleaseBuffers()
        {
            _bufferX?.Release(); _bufferX = null;
            _bufferY?.Release(); _bufferY = null;
            _bufferZ?.Release(); _bufferZ = null;
            _dataReady = false;
        }

        private void OnDrawGizmos()
        {
            if (!_dataReady) return;

            // Usa la matrice del transform per disegnare in world space
            Gizmos.matrix = Matrix4x4.TRS(_boundsCenter, Quaternion.identity, Vector3.one);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, _boundsSize);
        }
    }
}
