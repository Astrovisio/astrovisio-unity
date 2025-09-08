using System;
using System.Collections.Generic;
using CatalogData;
using TMPro;
using UnityEngine;

namespace Astrovisio
{
    public class RenderManager : MonoBehaviour
    {
        public static RenderManager Instance { get; private set; }

        [Header("Dependencies")]
        [SerializeField] private APIManager apiManager;
        [SerializeField] private ProjectManager projectManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Camera mainCamera;

        [Header("Other")]
        [SerializeField] private DataRenderer dataRendererPrefab;

        // Camera
        private Vector3 initialCameraTargetPosition;
        private Vector3 initialCameraRotation;
        private float initialCameraDistance;
        private OrbitCameraController orbitController;

        // Events
        // public event Action<KDTreeComponent> OnKDTreeComponentChanged;
        public event Action<Project> OnProjectRenderReady;
        public event Action<Project> OnProjectRenderStart;
        public event Action<Project> OnProjectRenderEnd;

        // Settings
        private DataRenderer dataRenderer;
        private KDTreeComponent kdTreeComponent;
        private ParamRenderSettings renderSettings;
        private Dictionary<Project, DataContainer> projectDataContainers = new();

        // Local
        public bool isInspectorModeActive = false;


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

            orbitController = mainCamera.GetComponent<OrbitCameraController>();

            if (orbitController != null)
            {
                initialCameraTargetPosition = orbitController.target.position;
                initialCameraRotation = orbitController.transform.rotation.eulerAngles;
                initialCameraDistance = Vector3.Distance(orbitController.transform.position, orbitController.target.position);
            }
        }

        public void SetDataInspector(bool state, bool bebugSphereVisibility)
        {
            // Debug.Log("UpdateDataInspector " + state);
            kdTreeComponent = dataRenderer.GetKDTreeComponent();
            kdTreeComponent.SetDataInspectorVisibility(bebugSphereVisibility);
            kdTreeComponent.realtime = state;
            isInspectorModeActive = state;

            if (!XRManager.Instance.IsVRActive)
            {
                Transform cameraTarget = FindAnyObjectByType<CameraTarget>().transform;
                dataRenderer.GetKDTreeComponent().controllerTransform = cameraTarget;
            }
            else
            {
                // VR Controller...
            }
        }

        private void ResetCameraTransform()
        {
            if (orbitController != null)
            {
                orbitController.ResetCameraView(initialCameraTargetPosition, initialCameraRotation, initialCameraDistance);
            }
        }

        private void OnProjectProcessed(Project project, DataPack pack)
        {
            DataContainer dataContainer = new DataContainer(pack, project);
            projectDataContainers[project] = dataContainer;
            OnProjectRenderReady?.Invoke(project);
            // Debug.Log("OnProjectReadyToGetRendered");
        }

        public DataRenderer GetCurrentDataRenderer()
        {
            return dataRenderer;
        }

        public void RenderDataContainer(Project project)
        {
            OnProjectRenderStart?.Invoke(project);

            ResetCameraTransform();

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
            
            SetDataInspector(false, true);

            OnProjectRenderEnd?.Invoke(project);
        }

        public void SetAxisSettings(AxisRenderSettings axisRenderSettings)
        {

            // Debug.Log($"SetAxisSettings: {axis} {thresholdMin} {thresholdMax} {scalingType}");
            dataRenderer.SetAxisAstrovisio(
                axisRenderSettings.Axis,
                axisRenderSettings.Name,
                axisRenderSettings.ThresholdMinSelected,
                axisRenderSettings.ThresholdMaxSelected,
                axisRenderSettings.ScalingType
            );
        }

        public void SetRenderSettings(ParamRenderSettings renderSettings)
        {
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

        public void SetAxisAstrovisio(Axis axis, string paramName, float thresholdMin, float thresholdMax, ScalingType scalingType)
        {
            if (dataRenderer is not null)
            {
                dataRenderer.SetAxisAstrovisio(axis, paramName, thresholdMin, thresholdMax, scalingType);
            }
        }

        private void SetColorMap(ParamRenderSettings renderSettings)
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

        private void SetOpacity(ParamRenderSettings renderSettings)
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

        public void SetNoise(bool state, float value = 0f)
        {
            AstrovisioDataSetRenderer astrovisioDataSetRenderer = dataRenderer.GetAstrovidioDataSetRenderer();
            astrovisioDataSetRenderer.SetNoise(state, value);
        }

    }

}
