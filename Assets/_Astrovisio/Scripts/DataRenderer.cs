using System.IO;
using CatalogData;
using UnityEngine;

namespace Astrovisio
{
    public class DataRenderer : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private Shader pointShader;
        [SerializeField] private GameObject dataCubeContainer;
        [SerializeField] private GameObject catalogDataSetRendererGO;
        [SerializeField] private GameObject astrovidioDataSetRendererGO;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private string fileName = "project_1_processed.csv";

        private DataContainer dataContainer;

        private void Start()
        {
            if (debugMode)
            {
                var dataPack = LoadCSV();
                if (dataPack == null)
                {
                    return;
                }

                dataContainer = new DataContainer(dataPack);
                RenderDataContainer(dataContainer);
            }
        }

        private DataPack LoadCSV()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

            if (!File.Exists(filePath))
            {
                Debug.LogError("File CSV non trovato: " + filePath);
                return null;
            }

            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 2)
            {
                Debug.LogWarning("CSV vuoto o senza dati.");
                return null;
            }

            // Header
            string[] headers = lines[0].Split(',');
            var pack = new DataPack
            {
                Columns = headers,
                Rows = new double[lines.Length - 1][]
            };

            // Dati
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                double[] row = new double[headers.Length];

                for (int j = 0; j < headers.Length; j++)
                    row[j] = (j < values.Length && double.TryParse(values[j], out double result)) ? result : double.NaN;

                pack.Rows[i - 1] = row;
            }

            Debug.Log($"[DataRenderer] Caricati {pack.Rows.Length} punti da {fileName}");
            return pack;
        }

        public void RenderDataContainer(DataContainer dataContainer)
        {
            // if (dataCubeContainer == null)
            // {
            //     Debug.LogError("DataCubeContainer not assigned.");
            //     return;
            // }

            // var cubeRenderer = dataCubeContainer.GetComponent<DataCubeRenderer>();
            // if (cubeRenderer == null)
            // {
            //     cubeRenderer = dataCubeContainer.AddComponent<DataCubeRenderer>();
            // }

            // cubeRenderer.Initialize(dataContainer, pointShader);

            int rowCount = dataContainer.DataPack.Rows.Length;
            float[] x = new float[rowCount];
            float[] y = new float[rowCount];
            float[] z = new float[rowCount];
            Debug.Log("A");
            for (int i = 0; i < rowCount; i++)
            {
                x[i] = (float)dataContainer.DataPack.Rows[i][0];
                y[i] = (float)dataContainer.DataPack.Rows[i][1];
                z[i] = (float)dataContainer.DataPack.Rows[i][2];
            }
            Debug.Log("B");

            AstrovidioDataSetRenderer astrovidioDataSetRenderer = astrovidioDataSetRendererGO.GetComponent<AstrovidioDataSetRenderer>();
            // if (catalogDataSetRenderer == null)
            // {
            //     cubeRenderer = dataCubeContainer.AddComponent<DataCubeRenderer>();
            // }

            Debug.Log(x);
            Debug.Log(y);
            Debug.Log(z);
            astrovidioDataSetRenderer.SetCatalogData(x, y, z);
            astrovidioDataSetRenderer.gameObject.SetActive(true);
        }

    }

}
