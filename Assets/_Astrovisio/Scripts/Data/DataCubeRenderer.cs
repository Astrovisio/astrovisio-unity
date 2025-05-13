using UnityEngine;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

public class DataCubeRenderer : MonoBehaviour
{
    private bool isDataAvailable = false;
    private int _pointCount;
    private Material _material;
    private ComputeBuffer _bufferX;
    private ComputeBuffer _bufferY;
    private ComputeBuffer _bufferZ;
    private ComputeBuffer _bufferSize;
    private ComputeBuffer _bufferRho;
    private int _currentColorMap = -1;

    [Header("Editor")]
    [SerializeField] private bool activateDebugMode = false;
    [SerializeField] private string dataFileName = "data.csv";
    [SerializeField] private int maxDataPoints = 1000;

    [Header("Parameter 1")]
    [SerializeField][Range(0.0f, 1.0f)] public float thresholdMin1 = 0.2f;
    [SerializeField][Range(0.0f, 1.0f)] public float thresholdMax1 = 0.8f;

    [Header("Color Map")]
    [SerializeField][Range(0, 5)] private int colorMapIndex = 0;
    [SerializeField] private ColorMapSOToDelete[] colorMapSOArray;

#if UNITY_EDITOR
    private void Start()
    {
        LoadDataDebug();
    }
#endif

    private void Update()
    {
        if (!isDataAvailable)
        {
            return;
        }

        if (_material != null)
        {
            _material.SetFloat("thresholdMin", thresholdMin1);
            _material.SetFloat("thresholdMax", thresholdMax1);
        }

        if (colorMapSOArray != null && colorMapSOArray.Length > 0 && colorMapIndex != _currentColorMap)
        {
            // Debug.Log(colorMapSOArray[colorMapIndex].colorMapID + " " + colorMapSOArray[colorMapIndex].colorMapName);
            UpdateColormap();
        }
    }

    private void UpdateColormap()
    {
        if (colorMapSOArray != null && colorMapSOArray.Length > colorMapIndex)
        {
            Texture2D colormapTexture = colorMapSOArray[colorMapIndex].GetExtractedTexture();
            _material.SetTexture("colormapTex", colormapTexture);
            _currentColorMap = colorMapIndex;
        }
    }

    private void OnRenderObject()
    {
        if (_material == null)
            return;

        _material.SetMatrix("datasetMatrix", transform.localToWorldMatrix);
        _material.SetFloat("scalingFactor", transform.localScale.x);

        _material.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Points, _pointCount);
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    public void LoadData(string data)
    {
        List<float> dataX = new List<float>();
        List<float> dataY = new List<float>();
        List<float> dataZ = new List<float>();
        List<float> dataSize = new List<float>();
        List<float> dataRho = new List<float>();

        string[] lines = data.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
        {
            Debug.LogError("CSV non contiene abbastanza righe.");
            return;
        }

        int endIndex = lines.Length;
#if UNITY_EDITOR
        if (activateDebugMode && maxDataPoints > 0)
        {
            endIndex = Mathf.Min(lines.Length, maxDataPoints + 1);
        }
#endif

        for (int i = 1; i < endIndex; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length >= 5)
            {
                dataX.Add(float.Parse(values[0], CultureInfo.InvariantCulture));
                dataY.Add(float.Parse(values[1], CultureInfo.InvariantCulture));
                dataZ.Add(float.Parse(values[2], CultureInfo.InvariantCulture));
                dataSize.Add(float.Parse(values[3], CultureInfo.InvariantCulture));
                dataRho.Add(float.Parse(values[4], CultureInfo.InvariantCulture));
            }
        }

        _pointCount = dataX.Count;
        // Debug.Log("Punti caricati dal CSV: " + _pointCount);

        ReleaseBuffers();

        _bufferX = new ComputeBuffer(_pointCount, sizeof(float));
        _bufferY = new ComputeBuffer(_pointCount, sizeof(float));
        _bufferZ = new ComputeBuffer(_pointCount, sizeof(float));
        _bufferSize = new ComputeBuffer(_pointCount, sizeof(float));
        _bufferRho = new ComputeBuffer(_pointCount, sizeof(float));

        _bufferX.SetData(dataX.ToArray());
        _bufferY.SetData(dataY.ToArray());
        _bufferZ.SetData(dataZ.ToArray());
        _bufferSize.SetData(dataSize.ToArray());
        _bufferRho.SetData(dataRho.ToArray());

        _material = new Material(Shader.Find("Hidden/SimplePointShader"));
        _material.SetBuffer("dataX", _bufferX);
        _material.SetBuffer("dataY", _bufferY);
        _material.SetBuffer("dataZ", _bufferZ);
        _material.SetBuffer("dataSize", _bufferSize);
        _material.SetBuffer("dataRho", _bufferRho);

        UpdateColormap();

        isDataAvailable = true;
    }

    private void LoadDataDebug()
    {
        if (activateDebugMode)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, dataFileName);
            if (File.Exists(filePath))
            {
                string csvData = File.ReadAllText(filePath);
                LoadData(csvData);
            }
            else
            {
                Debug.LogError("File CSV non trovato in StreamingAssets: " + filePath);
            }
        }
    }

    private void ReleaseBuffers()
    {
        if (_bufferX != null) { _bufferX.Release(); _bufferX = null; }
        if (_bufferY != null) { _bufferY.Release(); _bufferY = null; }
        if (_bufferZ != null) { _bufferZ.Release(); _bufferZ = null; }
        if (_bufferSize != null) { _bufferSize.Release(); _bufferSize = null; }
        if (_bufferRho != null) { _bufferRho.Release(); _bufferRho = null; }
    }

    public void SetThresholdMin(float newValue)
    {
        thresholdMin1 = newValue;
    }

    public void SetThresholdMax(float newValue)
    {
        thresholdMax1 = newValue;
    }

    public float GetThresholdMin()
    {
        return thresholdMin1;
    }

    public float GetThresholdMax()
    {
        return thresholdMax1;
    }

}
