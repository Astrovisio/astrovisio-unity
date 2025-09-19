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
        public event Action<Project> OnProjectRenderReady;
        public event Action<Project> OnProjectRenderStart;
        public event Action<Project> OnProjectRenderEnd;

        // Settings
        public RenderSettingsController RenderSettingsController { get; set; }
        public DataRenderer DataRenderer { get; set; }
        private KDTreeComponent kdTreeComponent;
        private ParamRenderSettings paramRenderSettings;

        // Local
        private Dictionary<File, DataContainer> fileDataContainers = new();
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
            projectManager.FileProcessed += OnProjectProcessed;

            orbitController = mainCamera.GetComponent<OrbitCameraController>();

            if (orbitController != null)
            {
                initialCameraTargetPosition = orbitController.target.position;
                initialCameraRotation = orbitController.transform.rotation.eulerAngles;
                initialCameraDistance = Vector3.Distance(orbitController.transform.position, orbitController.target.position);
            }

            RenderSettingsController = new RenderSettingsController();
        }

        public void SetDataInspector(bool state, bool bebugSphereVisibility)
        {
            // Debug.Log("UpdateDataInspector " + state);
            kdTreeComponent = DataRenderer.GetKDTreeComponent();
            kdTreeComponent.SetDataInspectorVisibility(bebugSphereVisibility);
            isInspectorModeActive = state;

            if (!XRManager.Instance.IsVRActive)
            {
                Transform cameraTarget = FindAnyObjectByType<CameraTarget>().transform;
                DataRenderer.GetKDTreeComponent().controllerTransform = cameraTarget;
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

        private void OnProjectProcessed(Project project, File file, DataPack pack)
        {
            DataContainer dataContainer = new DataContainer(pack, project, project.Files[0]); // GB
            fileDataContainers[file] = dataContainer;
            OnProjectRenderReady?.Invoke(project);
            // Debug.Log("OnProjectReadyToGetRendered");
        }

        public void RenderDataContainer(Project project, File file)
        {
            OnProjectRenderStart?.Invoke(project);

            ResetCameraTransform();

            DataContainer dataContainer = fileDataContainers[file];

            paramRenderSettings = null;

            DataRenderer[] allDataRenderer = FindObjectsByType<DataRenderer>(FindObjectsSortMode.None);
            foreach (DataRenderer dR in allDataRenderer)
            {
                Destroy(dR.gameObject);
            }

            // Debug.Log("Length :" + dataContainer.DataPack.Rows.Length);
            DataRenderer = Instantiate(dataRendererPrefab);
            RenderSettingsController.DataRenderer = DataRenderer;
            DataRenderer.RenderDataContainer(dataContainer);

            SetDataInspector(false, true);

            OnProjectRenderEnd?.Invoke(project);
        }

    }

}
