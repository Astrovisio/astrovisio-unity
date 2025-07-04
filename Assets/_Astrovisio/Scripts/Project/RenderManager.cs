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
        [SerializeField] private Camera mainCamera;

        [Header("Other")]
        [SerializeField] private DataRenderer dataRendererPrefab;
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;

        // Camera
        private Vector3 initialCameraTargetPosition;
        private Vector3 initialCameraRotation;
        private float initialCameraDistance;
        private OrbitCameraController orbitController;

        // Settings
        private DataRenderer dataRenderer;
        private ParamRenderSettings renderSettings;
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

            orbitController = mainCamera.GetComponent<OrbitCameraController>();

            if (orbitController != null)
            {
                initialCameraTargetPosition = orbitController.target.position;
                initialCameraRotation = orbitController.transform.rotation.eulerAngles;
                initialCameraDistance = Vector3.Distance(orbitController.transform.position, orbitController.target.position);
            }

            textMeshProUGUI.text = "test";
        }

        // TO BE REMOVED ON FUTURE
        private void Update()
        {
            // TO BE REMOVED ON FUTURE
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleDebugSphere();
            }
        }

        // TO BE REMOVED ON FUTURE
        public void ToggleDebugSphere()
        {
            GameObject foundObject = GameObject.Find("DebugNearestPointSphere");
            if (foundObject != null)
            {
                MeshRenderer meshRenderer = foundObject.GetComponent<MeshRenderer>();
                bool currentState = meshRenderer.enabled;
                meshRenderer.enabled = !currentState;
                // Debug.Log($"Toggled object '{foundObject.name}' to {(foundObject.activeSelf ? "visible" : "hidden")}");
            }
        }

        // TO BE REMOVED ON FUTURE
        public void SetDebugSphere(bool value)
        {
            GameObject foundObject = GameObject.Find("DebugNearestPointSphere");
            if (foundObject != null)
            {
                MeshRenderer meshRenderer = foundObject.GetComponent<MeshRenderer>();
                meshRenderer.enabled = value;
                // Debug.Log($"Toggled object '{foundObject.name}' to {(foundObject.activeSelf ? "visible" : "hidden")}");
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
            OnProjectReadyToGetRendered?.Invoke(project);
            // Debug.Log("OnProjectReadyToGetRendered");
        }

        public DataRenderer GetCurrentDataRenderer() => dataRenderer;

        public void RenderDataContainer(Project project)
        {
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
            // Debug.Log("RenderDataContainer -> Nuovo DataRenderer instanziato e dati renderizzati.");

            SetDebugSphere(false);
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

        // private void SetNone()
        // {
        //     dataRenderer.SetNone();
        // }

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

    }

}
