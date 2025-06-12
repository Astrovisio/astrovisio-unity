using System;
using System.Collections;
using System.Collections.Generic;
using CatalogData;
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

        private Dictionary<Project, DataContainer> projectDataContainers = new();

        public Action<Project> OnProjectReadyToGetRendered;


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

        private void Start()
        {
            projectManager.ProjectProcessed += OnProjectProcessed;
        }

        private void OnProjectProcessed(Project project, DataPack pack)
        {
            DataContainer dataContainer = new DataContainer(pack, project);
            projectDataContainers[project] = dataContainer;
            OnProjectReadyToGetRendered?.Invoke(project);
            // Debug.Log("OnProjectReadyToGetRendered");
        }

        public DataRenderer GetCurrentDataRenderer() => dataRenderer;

        public void RenderDataContainer(Project project)
        {
            DataContainer dataContainer = projectDataContainers[project];

            renderSettings = null;

            DataRenderer[] allDataRenderer = FindObjectsByType<DataRenderer>(FindObjectsSortMode.None);
            foreach (DataRenderer dR in allDataRenderer)
            {
                Destroy(dR.gameObject);
            }

            // Debug.Log("Length :" + dataContainer.DataPack.Rows.Length);
            dataRenderer = Instantiate(dataRendererPrefab);
            dataRenderer.RenderDataContainer(dataContainer);
            // Debug.Log("RenderDataContainer -> Nuovo DataRenderer instanziato e dati renderizzati.");
        }


        public void SetRenderSettings(RenderSettings renderSettings)
        {
            // if (renderSettings.Mapping == MappingType.None && renderSettings.MappingSettings is null)
            // {
            //     Debug.Log("SetRenderSettings -> None");
            //     SetNone();
            // }
            // else
            if (renderSettings.Mapping == MappingType.Opacity && renderSettings.MappingSettings is OpacitySettings)
            {
                // Debug.Log("SetRenderSettings -> Opacity " + renderSettings.MappingSettings.ScalingType);
                SetOpacity(renderSettings);
            }
            else if (renderSettings.Mapping == MappingType.Colormap && renderSettings.MappingSettings is ColorMapSettings)
            {
                // Debug.Log("SetRenderSettings -> Colormap");
                SetColorMap(renderSettings);
            }
        }

        private void SetNone()
        {
            dataRenderer.SetNone();
        }

        private void SetColorMap(RenderSettings renderSettings)
        {
            if (renderSettings.Mapping == MappingType.Colormap && renderSettings.MappingSettings is ColorMapSettings)
            {
                ColorMapSettings colorMapSettings = renderSettings.MappingSettings as ColorMapSettings;

                string name = renderSettings.Name;
                ColorMapEnum colorMap = colorMapSettings.ColorMap;
                float thresholdMinSelected = colorMapSettings.ThresholdMinSelected;
                float thresholdMaxSelected = colorMapSettings.ThresholdMaxSelected;
                ScalingType scalingType = colorMapSettings.ScalingType;
                bool invert = colorMapSettings.Invert;

                dataRenderer.SetColorMap(name, colorMap, thresholdMinSelected, thresholdMaxSelected, scalingType, invert);
            }
            else
            {
                Debug.Log("Error on renderSettings.Mapping");
                return;
            }
        }

        public void RemoveColorMap()
        {
            dataRenderer.RemoveColorMap();
        }

        private void SetOpacity(RenderSettings renderSettings)
        {
            if (renderSettings.Mapping == MappingType.Opacity && renderSettings.MappingSettings is OpacitySettings)
            {
                OpacitySettings opacitySettings = renderSettings.MappingSettings as OpacitySettings;

                string name = renderSettings.Name;

                dataRenderer.SetOpacity(name, opacitySettings.ThresholdMinSelected, opacitySettings.ThresholdMaxSelected, opacitySettings.ScalingType, opacitySettings.Invert);
            }
            else
            {
                Debug.Log("Error on renderSettings.Mapping");
                return;
            }
        }

        public void RemoveOpacity()
        {
            dataRenderer.RemoveOpacity();
        }

    }

}
