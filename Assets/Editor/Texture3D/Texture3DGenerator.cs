using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

public class Texture3DGenerator : ScriptableObject
{

    [MenuItem("Texture3D/Generate Texture3D from CSV")]
    public static void LoadCSVAndGenerateTexture()
    {
        string fileName = "data.csv";
        int textureSize = 256;
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError("File CSV non trovato: " + path);
            return;
        }

        string[] lines = File.ReadAllLines(path);
        List<Vector3> dataPoints = new List<Vector3>();
        List<float> dataSizes = new List<float>();
        List<float> dataRho = new List<float>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.StartsWith("x"))
                continue;

            string[] values = line.Split(',');

            if (values.Length < 5)
                continue;

            float x = float.Parse(values[0]);
            float y = float.Parse(values[1]);
            float z = float.Parse(values[2]);
            float size = float.Parse(values[3]);
            float rho = float.Parse(values[4]);

            Debug.Log($"Indice: {i}, Valori: x={x}, y={y}, z={z}, size={size}, rho={rho}");
            dataPoints.Add(new Vector3(x, y, z));
            dataSizes.Add(size);
            dataRho.Add(rho);
        }

        GenerateTexture3D(dataPoints, dataSizes, dataRho, textureSize);
    }

    private static void GenerateTexture3D(List<Vector3> dataPoints, List<float> dataSizes, List<float> dataRho, int textureSize)
    {
        Texture3D volumeTexture = new Texture3D(textureSize, textureSize, textureSize, TextureFormat.RGBAFloat, false);
        volumeTexture.wrapMode = TextureWrapMode.Clamp;
        volumeTexture.filterMode = FilterMode.Bilinear;

        Color[] colorData = new Color[textureSize * textureSize * textureSize];

        for (int i = 0; i < dataPoints.Count; i++)
        {
            Vector3 point = dataPoints[i];
            float size = dataSizes[i];
            float rho = dataSizes[i];

            int xi = Mathf.Clamp(Mathf.RoundToInt((point.x + 1) * (textureSize / 2)), 0, textureSize - 1);
            int yi = Mathf.Clamp(Mathf.RoundToInt((point.y + 1) * (textureSize / 2)), 0, textureSize - 1);
            int zi = Mathf.Clamp(Mathf.RoundToInt((point.z + 1) * (textureSize / 2)), 0, textureSize - 1);

            int index = xi + yi * textureSize + zi * textureSize * textureSize;
            colorData[index] = new Color(rho, 0.0f, 0.0f, 1.0f);
        }

        volumeTexture.SetPixels(colorData);
        volumeTexture.Apply();

        string timestamp = DateTime.Now.ToString("HHmmss");
        string savePath = $"Assets/TestGB/{timestamp}.asset";
        AssetDatabase.CreateAsset(volumeTexture, savePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Texture3D salvata in: " + savePath);
    }

}
