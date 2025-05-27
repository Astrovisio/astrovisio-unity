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

        public void RenderDataContainer(DataContainer dataContainer)
        {
            renderSettings = null;
            tempRenderSettings = null;

            DataRenderer[] allDataRenderer = FindObjectsByType<DataRenderer>(FindObjectsSortMode.None);
            foreach (DataRenderer dR in allDataRenderer)
            {
                Destroy(dR.gameObject);
            }

            DataRenderer dataRenderer = Instantiate(dataRendererPrefab);
            dataRenderer.RenderDataContainer(dataContainer);
        }

        public void InitSettings(string paramName, string colorMapName, float lowLimit, float highLimit, float minValue, float maxValue)
        {
            // Debug.Log("InitSettings: " + colorMapName + " " + lowLimit + " " + highLimit + " " + minValue + " " + maxValue);

            if (renderSettings is null)
            {
                Debug.Log("InitSettings -> RenderSettings");
                renderSettings = new RenderSettings
                (
                    paramName,
                    colorMapName,
                    lowLimit,
                    highLimit,
                    minValue,
                    maxValue
                );
            }
            else
            {
                Debug.Log("InitSettings -> TempRenderSettings");
                tempRenderSettings = new RenderSettings
                (
                    paramName,
                    colorMapName,
                    lowLimit,
                    highLimit,
                    minValue,
                    maxValue
                );
            }

        }

        public void ApplySettings()
        {
            renderSettings = tempRenderSettings;
            tempRenderSettings = null;
        }

        public void SetColorMap(string colorMap)
        {
            Debug.Log("SetColorMap: " + colorMap);
            renderSettings.ColorMapName = colorMap;
            // Eventualmente carica lo sprite da nome mappa se serve
            // renderSettings.ColorMap = LoadColorMapSprite(colorMap);
        }

        public void SetThresholdValues(string paramName, float minValue, float maxValue)
        {
            if (tempRenderSettings is null)
            {
                tempRenderSettings = new RenderSettings(
                    paramName,
                    renderSettings.ColorMapName,
                    renderSettings.LowLimit,
                    renderSettings.HighLimit,
                    minValue,
                    maxValue
                );
            }

            tempRenderSettings.ParamName = paramName;
            tempRenderSettings.MinValue = minValue;
            tempRenderSettings.MaxValue = maxValue;

            Debug.Log("SetThresholdValues: " + paramName + " " + minValue + " " + maxValue);
        }

    }
}
