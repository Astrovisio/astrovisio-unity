using System;
using System.IO;
using CatalogData;
using UnityEngine;

namespace Astrovisio
{
    public class DataRenderer : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private AstrovisioDataSetRenderer astrovidioDataSetRenderer;
        [SerializeField] private KDTreeComponent kdTreeComponent;
        [SerializeField] private AxesCanvasHandler axesCanvasHandler;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private string fileName = "project_1_processed.csv";


        private DataContainer dataContainer;

        private Vector3 dataRendererOriginalPosition;
        private Quaternion dataRendererOriginalRotation;
        private Vector3 dataRendererOriginalScale;


        private void Start()
        {
            if (debugMode)
            {
                DataPack dataPack = LoadCSV();
                if (dataPack == null)
                {
                    return;
                }

                dataContainer = new DataContainer(dataPack, null, null);
                RenderDataContainer(dataContainer);
            }

            dataRendererOriginalPosition = astrovidioDataSetRenderer.transform.position;
            dataRendererOriginalRotation = astrovidioDataSetRenderer.transform.rotation;
            dataRendererOriginalScale = astrovidioDataSetRenderer.transform.localScale;
        }

        public DataContainer GetDataContainer() => dataContainer;
        public AstrovisioDataSetRenderer GetAstrovidioDataSetRenderer() => astrovidioDataSetRenderer;
        public KDTreeComponent GetKDTreeComponent() => kdTreeComponent;

        private DataPack LoadCSV()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError("File CSV non trovato: " + filePath);
                return null;
            }

            string[] lines = System.IO.File.ReadAllLines(filePath);
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
                {
                    row[j] = (j < values.Length && double.TryParse(values[j], out double result)) ? result : double.NaN;
                }


                pack.Rows[i - 1] = row;
            }

            Debug.Log($"[DataRenderer] Caricati {pack.Rows.Length} punti da {fileName}");
            return pack;
        }

        public void RenderDataContainer(DataContainer dataContainer)
        {
            this.dataContainer = dataContainer;
            astrovidioDataSetRenderer.SetCatalogData(dataContainer, debugMode);
            astrovidioDataSetRenderer.gameObject.SetActive(true);

            axesCanvasHandler.SetAxesLabel(dataContainer.XAxisName, dataContainer.YAxisName, dataContainer.ZAxisName);
        }

        public void SetAxisAstrovisio(Axis axis, string paramName, float thresholdMin, float thresholdMax, ScalingType scalingType)
        {
            if (astrovidioDataSetRenderer is not null)
            {
                astrovidioDataSetRenderer.SetAxisAstrovisio(axis, paramName, thresholdMin, thresholdMax, scalingType);
            }
        }

        public void SetNone()
        {
            if (astrovidioDataSetRenderer is not null)
            {
                astrovidioDataSetRenderer.SetNoneAstrovisio();
            }
        }

        public void SetColormap(string paramName, ColorMapEnum colorMap, float min, float max, ScalingType scalingType, bool inverseMapping)
        {
            if (astrovidioDataSetRenderer is not null)
            {
                astrovidioDataSetRenderer.SetColorMapAstrovisio(paramName, colorMap, min, max, scalingType, inverseMapping);
            }
        }

        public void RemoveColormap()
        {
            astrovidioDataSetRenderer.RemoveColorMapAstrovisio();
        }

        public void SetOpacity(string paramName, float min, float max, ScalingType scalingType, bool inverseMapping)
        {
            if (astrovidioDataSetRenderer is not null)
            {
                astrovidioDataSetRenderer.SetOpacityAstrovisio(paramName, min, max, scalingType, inverseMapping);
            }
        }

        public void RemoveOpacity()
        {
            astrovidioDataSetRenderer.RemoveOpacityAstrovisio();
        }

        public void ResetDatasetTransform()
        {
            astrovidioDataSetRenderer.transform.position = dataRendererOriginalPosition;
            astrovidioDataSetRenderer.transform.rotation = dataRendererOriginalRotation;
            astrovidioDataSetRenderer.transform.localScale = dataRendererOriginalScale;
        }

    }

}
