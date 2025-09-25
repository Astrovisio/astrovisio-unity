using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astrovisio
{

    public readonly struct ProjectFile : IEquatable<ProjectFile>
    {
        public readonly int ProjectID;
        public readonly int FileID;

        public ProjectFile(int projectID, int fileID)
        {
            ProjectID = projectID;
            FileID = fileID;
        }

        public bool Equals(ProjectFile other) =>
            ProjectID == other.ProjectID && FileID == other.FileID;

        public override bool Equals(object obj) =>
            obj is ProjectFile other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ProjectID;
                hash = hash * 31 + FileID;
                return hash;
            }
        }

        public override string ToString() => $"P{ProjectID}-F{FileID}";
    }

    public class RenderManager : MonoBehaviour
    {
        public static RenderManager Instance { get; private set; }

        [Header("Dependencies")]
        [SerializeField] private APIManager apiManager;
        [SerializeField] private ProjectManager projectManager;
        [SerializeField] private UIManager uiManager;


        [Header("Other")]
        [SerializeField] private DataRenderer dataRendererPrefab;

        // Events
        public event Action<Project> OnProjectRenderReady;
        public event Action<Project> OnFileRenderStart;
        public event Action<Project> OnFileRenderEnd;

        // Settings
        public RenderSettingsController RenderSettingsController { get; set; }
        public DataRenderer DataRenderer { get; set; }
        private KDTreeComponent kdTreeComponent;
        private ParamRenderSettings paramRenderSettings;

        // Local
        public bool isInspectorModeActive = false;
        private readonly Dictionary<ProjectFile, DataContainer> processedFileDictionary = new();


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
            projectManager.FileUpdated += OnFileUpdated;
            projectManager.FileProcessed += OnFileProcessed;

            RenderSettingsController = new RenderSettingsController();
        }




        // === Add / Get / Remove / Clear ===
        private static ProjectFile Key(int projectId, int fileId)
        {
            return new ProjectFile(projectId, fileId);
        }

        public void RegisterProcessedFile(Project project, File file, DataPack pack)
        {
            if (project == null || file == null || pack == null)
            {
                Debug.LogError("RegisterProcessedFile: null.");
                return;
            }

            DataContainer dataContainer = new DataContainer(pack, project, file);
            processedFileDictionary[Key(project.Id, file.Id)] = dataContainer;

            OnProjectRenderReady?.Invoke(project);
        }

        public bool TryGetDataContainer(int projectId, int fileId, out DataContainer dc)
        {
            return processedFileDictionary.TryGetValue(Key(projectId, fileId), out dc);
        }

        public bool TryGetDataContainer(Project project, File file, out DataContainer dc)
        {
            return processedFileDictionary.TryGetValue(Key(project.Id, file.Id), out dc);
        }

        public bool RemoveProcessedFile(int projectId, int fileId)
        {
            return processedFileDictionary.Remove(Key(projectId, fileId));
        }

        public int RemoveProcessedProject(int projectId)
        {
            var toRemove = new List<ProjectFile>();
            foreach (var k in processedFileDictionary.Keys)
                if (k.ProjectID == projectId) toRemove.Add(k);

            foreach (var k in toRemove)
                processedFileDictionary.Remove(k);

            return toRemove.Count;
        }

        public void ClearAllProcessed()
        {
            processedFileDictionary.Clear();
        }


        // === Render ===
        public void RenderFile(int projectId, int fileId)
        {
            if (!TryGetDataContainer(projectId, fileId, out var dc))
            {
                Debug.LogWarning($"RenderDataContainer: no DataContainer for P{projectId} F{fileId}");
                return;
            }
            RenderDataContainer(dc);
            // Debug.Log($"Rendering project id {projectId}, file id {fileId}");
        }

        public void RenderFile(Project project, File file)
        {
            if (!TryGetDataContainer(project, file, out var dc))
            {
                Debug.LogWarning($"RenderDataContainer: DataContainer not found for P{project?.Id} F{file?.Id}");
                return;
            }
            RenderDataContainer(dc);
            // Debug.Log($"Rendering project {project?.Name}, file {file?.Name}");
        }

        private void RenderDataContainer(DataContainer dc)
        {
            if (dc == null)
            {
                Debug.LogError("RenderDataContainer: DataContainer is null.");
                return;
            }

            // If DataContainer exposes Project, use it for events/camera
            Project project = dc.Project;
            OnFileRenderStart?.Invoke(project);

            SceneManager.Instance.ResetCameraTransform();

            paramRenderSettings = null;

            if (DataRenderer != null)
            {
                Destroy(DataRenderer.gameObject);
                DataRenderer = null;
            }

            DataRenderer = Instantiate(dataRendererPrefab);
            RenderSettingsController.DataRenderer = DataRenderer;

            DataRenderer.RenderDataContainer(dc);

            SetDataInspector(false, true);
            OnFileRenderEnd?.Invoke(project);
        }


        // === Events ===
        private void OnFileUpdated(Project project, File file)
        {
            if (project == null || file == null)
            {
                Debug.LogError("OnFileUpdated: null project or file.");
                return;
            }

            bool removed = RemoveProcessedFile(project.Id, file.Id);

            if (removed)
            {
                Debug.Log($"Removed processed data for Project {project.Name}, File {file.Name} (must be reprocessed).");
            }
            else
            {
                Debug.Log($"No processed data found to remove for Project {project.Name}, File {file.Name}.");
            }
        }

        private void OnFileProcessed(Project project, File file, DataPack pack)
        {
            // Debug.Log($"Processed project {project.Name}, file {file.Name}, with {pack.Rows.Length} points.");
            // Debug.Log($"C {file.Id} {file.Name} {file.Processed}");
            RegisterProcessedFile(project, file, pack);
        }


        // TODO: fix this method if there is any error GB
        public void SetDataInspector(bool state, bool debugSphereVisibility)
        {
            // Debug.Log("UpdateDataInspector " + state);
            kdTreeComponent = DataRenderer.GetKDTreeComponent();
            kdTreeComponent.SetDataInspectorVisibility(debugSphereVisibility);
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

    }

}
