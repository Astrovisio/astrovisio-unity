using UnityEngine;

namespace Astrovisio
{
    public class RenderManager : MonoBehaviour
    {
        public static RenderManager Instance { get; private set; }

        [Header("Dependencies")]
        [SerializeField] private APIManager apiManager;
        [SerializeField] private ProjectManager projectManager;

        [Header("Other")]
        [SerializeField] private DataRenderer dataRendererPrefab;

        // Settings
        private DataRenderer dataRenderer;
        private RenderSettings renderSettings;
        private RenderSettings tempRenderSettings;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of RenderManager found. Destroying the new one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public DataRenderer GetCurrentDataRenderer() => dataRenderer;

        public void RenderDataContainer(DataContainer dataContainer)
        {
            renderSettings = null;
            tempRenderSettings = null;

            DataRenderer[] allDataRenderer = FindObjectsByType<DataRenderer>(FindObjectsSortMode.None);
            foreach (DataRenderer dR in allDataRenderer)
            {
                Destroy(dR.gameObject);
            }

            dataRenderer = Instantiate(dataRendererPrefab);
            dataRenderer.RenderDataContainer(dataContainer);
            Debug.Log("RenderDataContainer -> Nuovo DataRenderer instanziato e dati renderizzati.");
        }

        public void SetColorMap(string paramName, ColorMapEnum colorMap, float min, float max)
        {
            Debug.Log($"[Preview] SetColorMap TEMP → Param: {paramName}, ColorMap: {colorMap}, Min: {min}, Max: {max}");
            dataRenderer.SetColorMap(paramName, colorMap, min, max);

            if (renderSettings != null)
            {
                tempRenderSettings = new RenderSettings(
                    paramName,
                    colorMap.ToString(),
                    colorMap,
                    min,
                    max,
                    min,
                    max
                );
                Debug.Log("[TempRenderSettings] Aggiornato per anteprima.");
            }
        }

        public void SetColorMapThreshold(float min, float max)
        {
            Debug.Log($"[Preview] SetColorMapThreshold TEMP → Min: {min}, Max: {max}");
            dataRenderer.SetColorMapThreshold(min, max);

            if (renderSettings != null && tempRenderSettings != null)
            {
                tempRenderSettings = new RenderSettings(
                    tempRenderSettings.ParamName,
                    tempRenderSettings.ColorMapName,
                    tempRenderSettings.ColorMap,
                    tempRenderSettings.LowLimit,
                    tempRenderSettings.HighLimit,
                    min,
                    max
                );
                Debug.Log("[TempRenderSettings] Threshold aggiornato per anteprima.");
            }
        }

        public void InitSettings(string paramName, string colorMapName, ColorMapEnum colorMap, float lowLimit, float highLimit, float minValue, float maxValue)
        {
            if (renderSettings is null)
            {
                Debug.Log("[InitSettings] Creazione RenderSettings iniziali.");
                renderSettings = new RenderSettings(
                    paramName,
                    colorMapName,
                    colorMap,
                    lowLimit,
                    highLimit,
                    minValue,
                    maxValue
                );
                SetColorMap(renderSettings.ParamName, renderSettings.ColorMap, renderSettings.LowLimit, renderSettings.HighLimit);
                SetColorMapThreshold(renderSettings.MinValue, renderSettings.MaxValue);
            }
            else
            {
                Debug.Log("[InitSettings] Aggiornamento TempRenderSettings.");
                tempRenderSettings = new RenderSettings(
                    paramName,
                    colorMapName,
                    colorMap,
                    lowLimit,
                    highLimit,
                    minValue,
                    maxValue
                );
                SetColorMap(tempRenderSettings.ParamName, tempRenderSettings.ColorMap, tempRenderSettings.LowLimit, tempRenderSettings.HighLimit);
                SetColorMapThreshold(tempRenderSettings.MinValue, tempRenderSettings.MaxValue);
            }
        }

        public void ApplySettings()
        {
            if (tempRenderSettings != null)
            {
                Debug.Log("[ApplySettings] Impostazioni applicate:");
                Debug.Log($" → Param: {tempRenderSettings.ParamName}, ColorMap: {tempRenderSettings.ColorMap}, Min: {tempRenderSettings.LowLimit}, Max: {tempRenderSettings.HighLimit}");
                renderSettings = tempRenderSettings;
                tempRenderSettings = null;
            }
            else
            {
                Debug.LogWarning("[ApplySettings] Nessuna impostazione temporanea da applicare.");
            }
        }

        public void CancelSettings()
        {
            if (renderSettings != null)
            {
                Debug.Log("[CancelSettings] Ripristino impostazioni precedenti:");
                Debug.Log($" → Param: {renderSettings.ParamName}, ColorMap: {renderSettings.ColorMap}, Min: {renderSettings.LowLimit}, Max: {renderSettings.HighLimit}");

                SetColorMap(renderSettings.ParamName, renderSettings.ColorMap, renderSettings.LowLimit, renderSettings.HighLimit);
                SetColorMapThreshold(renderSettings.MinValue, renderSettings.MaxValue);
            }
            else
            {
                Debug.LogWarning("[CancelSettings] Nessuna impostazione salvata da ripristinare.");
            }

            tempRenderSettings = null;
        }

        // public void SetColorMap(string colorMap)
        // {
        //     Debug.Log("SetColorMap: " + colorMap);
        //     renderSettings.ColorMapName = colorMap;
        //     // Eventualmente carica lo sprite da nome mappa se serve
        //     // renderSettings.ColorMap = LoadColorMapSprite(colorMap);
        // }

        // public void SetThresholdValues(string paramName, float minValue, float maxValue)
        // {
        //     if (tempRenderSettings is null)
        //     {
        //         tempRenderSettings = new RenderSettings(
        //             paramName,
        //             renderSettings.ColorMapName,
        //             renderSettings.LowLimit,
        //             renderSettings.HighLimit,
        //             minValue,
        //             maxValue
        //         );
        //     }

        //     tempRenderSettings.ParamName = paramName;
        //     tempRenderSettings.MinValue = minValue;
        //     tempRenderSettings.MaxValue = maxValue;

        //     Debug.Log("SetThresholdValues: " + paramName + " " + minValue + " " + maxValue);
        // }
    }
}
