using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using System;

public class CSVLoader : MonoBehaviour
{
    [SerializeField] private DataCubeRenderer dataCubeRenderer;

    public void OnClickLoadCSV()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Seleziona un file CSV", "", "csv", false);
        if (paths.Length > 0)
        {
            string filePath = paths[0];
            string csvContent = LoadCSVFile(filePath);
            dataCubeRenderer.LoadData(csvContent);
        }
    }

    private string LoadCSVFile(string path)
    {
        string csvContent = string.Empty;

        try
        {
            if (File.Exists(path))
            {
                csvContent = File.ReadAllText(path);
            }
            else
            {
                Debug.LogError("Il file CSV non esiste: " + path);
            }
        }
        catch (IOException ex)
        {
            Debug.LogError("Errore durante la lettura del file: " + ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.LogError("Accesso negato al file: " + ex.Message);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Errore imprevisto: " + ex.Message);
        }

        return csvContent;
    }

}
