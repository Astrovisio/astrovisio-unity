using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatalogData;
using UnityEngine;

namespace Astrovisio
{
    [Serializable]
    public readonly struct ProjectFile : IEquatable<ProjectFile>
    {

        private readonly int projectId;
        private readonly int fileId;

        public int ProjectId
        {
            get => projectId;
        }

        public int FileId
        {
            get => fileId;
        }

        public ProjectFile(int projectId, int fileId)
        {
            this.projectId = projectId;
            this.fileId = fileId;
        }

        public bool Equals(ProjectFile other)
        {
            return ProjectId == other.ProjectId && FileId == other.FileId;
        }

        public override bool Equals(object obj)
        {
            return obj is ProjectFile other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ProjectId;
                hash = hash * 31 + FileId;
                return hash;
            }
        }

        public override string ToString()
        {
            return $"P{ProjectId}-F{FileId}";
        }
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

        // === Events ===
        public event Action<Project, File> OnFileRenderStart;
        public event Action<Project, File> OnFileRenderEnd;

        // === Settings ===
        // public RenderSettingsController RenderSettingsController { get; set; }
        public DataRenderer DataRenderer { get; set; }
        private KDTreeComponent kdTreeComponent;
        private ParamRenderSettings paramRenderSettings;

        // === Local ===
        public bool isInspectorModeActive = false;
        public Project renderedProject = null;
        public File renderedFile = null;

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

        // === DataContainer ===
        public bool TryGetDataContainer(int projectId, int fileId, out DataContainer dc)
        {
            if (ReelManager.Instance == null)
            {
                dc = null;
                return false;
            }

            return ReelManager.Instance.TryGetDataContainer(projectId, fileId, out dc);
        }

        public bool TryGetDataContainer(Project project, File file, out DataContainer dc)
        {
            if (project == null || file == null)
            {
                dc = null;
                return false;
            }

            if (ReelManager.Instance == null)
            {
                dc = null;
                return false;
            }

            return ReelManager.Instance.TryGetDataContainer(project.Id, file.Id, out dc);
        }

        // === Reel ===
        public bool TryGetReel(int projectId, out ProjectReel reel)
        {
            if (ReelManager.Instance == null)
            {
                reel = null;
                return false;
            }

            return ReelManager.Instance.TryGetReel(projectId, out reel);
        }

        public IReadOnlyList<int> GetReelOrderedIds(int projectId)
        {
            if (ReelManager.Instance == null)
            {
                return Array.Empty<int>();
            }

            return ReelManager.Instance.GetReelOrderedIds(projectId);
        }

        public void SetReelOrder(int projectId, IReadOnlyList<int> orderedIds)
        {
            if (ReelManager.Instance == null)
            {
                return;
            }

            Debug.Log($"[RenderManager] SetReelOrder P{projectId} → [{(orderedIds == null ? "∅" : string.Join(",", orderedIds))}]");
            ReelManager.Instance.SetReelOrder(projectId, orderedIds);
        }

        public bool RemoveFromReel(int projectId, int fileId)
        {
            if (ReelManager.Instance == null)
            {
                return false;
            }

            return ReelManager.Instance.RemoveFromReel(projectId, fileId);
        }

        public void ClearReel(int projectId)
        {
            if (ReelManager.Instance == null)
            {
                return;
            }

            ReelManager.Instance.ClearReel(projectId);
        }

        public void RemoveReel(int projectId)
        {
            if (ReelManager.Instance == null)
            {
                return;
            }

            ReelManager.Instance.RemoveReel(projectId);
        }

        public void RenderReelCurrent(int projectId)
        {
            if (ReelManager.Instance == null)
            {
                Debug.LogWarning($"[RenderManager] RenderReelCurrent: ReelManager missing for P{projectId}");
                return;
            }

            DataContainer dc = ReelManager.Instance.GetReelCurrentDataContainer(projectId);
            if (dc == null)
            {
                Debug.LogWarning($"[RenderManager] RenderReelCurrent: empty or invalid reel for P{projectId}");
                return;
            }

            string name = dc?.File?.Name ?? $"F{dc?.File?.Id}";
            // Debug.Log($"[RenderManager] RenderReelCurrent P{projectId} → {name}");
            RenderDataContainer(dc);
        }

        public void RenderReelNext(int projectId)
        {
            if (ReelManager.Instance == null)
            {
                Debug.LogWarning($"[RenderManager] RenderReelNext: ReelManager missing for P{projectId}");
                return;
            }

            if (projectManager.GetLocalProject(projectId).Files.Count <= 1)
            {
                Debug.LogWarning($"[RenderManager] RenderReelNext: not many file into P{projectId}");
                return;
            }

            int fileId = ReelManager.Instance.MoveNext(projectId);
            if (fileId < 0)
            {
                return;
            }

            if (!ReelManager.Instance.TryGetDataContainer(projectId, fileId, out var dc))
            {
                Debug.LogWarning($"[RenderManager] RenderReelNext: DataContainer not found for P{projectId} F{fileId}");
                return;
            }

            string name = dc?.File?.Name ?? $"F{fileId}";
            Debug.Log($"[RenderManager] RenderReelNext P{projectId} → {name} (F{fileId})");
            RenderDataContainer(dc);
        }

        public void RenderReelPrev(int projectId)
        {
            if (ReelManager.Instance == null)
            {
                Debug.LogWarning($"[RenderManager] RenderReelPrev: ReelManager missing for P{projectId}");
                return;
            }

            if (projectManager.GetLocalProject(projectId).Files.Count <= 1)
            {
                Debug.LogWarning($"[RenderManager] RenderReelPrev: not many file into P{projectId}");
                return;
            }

            int fileId = ReelManager.Instance.MovePrev(projectId);
            if (fileId < 0)
            {
                return;
            }

            if (!ReelManager.Instance.TryGetDataContainer(projectId, fileId, out var dc))
            {
                Debug.LogWarning($"[RenderManager] RenderReelPrev: DataContainer not found for P{projectId} F{fileId}");
                return;
            }

            string name = dc?.File?.Name ?? $"F{fileId}";
            Debug.Log($"[RenderManager] RenderReelPrev P{projectId} → {name} (F{fileId})");
            RenderDataContainer(dc);
        }

        public int? GetReelCurrentFileId(int projectId)
        {
            if (ReelManager.Instance == null)
            {
                return null;
            }

            return ReelManager.Instance.GetReelCurrentFileId(projectId);
        }

        // === Render ===
        public void RenderFile(int projectId, int fileId)
        {
            if (!TryGetDataContainer(projectId, fileId, out var dc))
            {
                Debug.LogWarning($"[RenderManager] RenderDataContainer: no DataContainer for P{projectId} F{fileId}");
                return;
            }

            string name = dc?.File?.Name ?? $"F{fileId}";
            Debug.Log($"[RenderManager] RenderFile direct P{projectId} → {name} (F{fileId})");
            RenderDataContainer(dc);
        }

        public void RenderFile(Project project, File file)
        {
            if (!TryGetDataContainer(project, file, out var dc))
            {
                Debug.LogWarning($"[RenderManager] RenderDataContainer: DataContainer not found for P{project?.Id} F{file?.Id}");
                return;
            }

            string name = dc?.File?.Name ?? $"F{file?.Id}";
            Debug.Log($"[RenderManager] RenderFile direct P{project?.Id} → {name} (F{file?.Id})");
            RenderDataContainer(dc);
        }

        private void RenderDataContainer(DataContainer dc)
        {
            if (dc == null)
            {
                Debug.LogError("[RenderManager] RenderDataContainer: DataContainer is null.");
                return;
            }

            Project project = dc.Project;
            File file = dc.File;
            OnFileRenderStart?.Invoke(project, file);

            // SceneManager.Instance.ResetCameraTransform();

            paramRenderSettings = null;
            ClearDataContainer();

            DataRenderer = Instantiate(dataRendererPrefab);
            // RenderSettingsController.DataRenderer = DataRenderer;

            string name = dc?.File?.Name ?? $"F{dc?.File?.Id}";
            // Debug.Log($"[RenderManager] RenderDataContainer P{project?.Id} → {name}");

            DataRenderer.RenderDataContainer(dc);

            SetNoise(0f);
            SetDataInspector(false, true);
            UpdateRenderedProjectFile(project, file);
            OnFileRenderEnd?.Invoke(project, file);
        }

        public void ClearDataContainer()
        {
            if (DataRenderer != null)
            {
                Destroy(DataRenderer.gameObject);
                DataRenderer = null;
            }
        }

        private void UpdateRenderedProjectFile(Project project, File file)
        {
            renderedProject = project;
            renderedFile = file;
        }

        // === Inspector ===
        public void SetDataInspector(bool state, bool debugSphereVisibility)
        {
            // kdTreeComponent = DataRenderer.GetKDTreeComponent();
            // kdTreeComponent.SetDataInspectorVisibility(debugSphereVisibility);
            // isInspectorModeActive = state;

            // if (!XRManager.Instance.IsVRActive)
            // {
            //     Transform cameraTarget = FindAnyObjectByType<CameraTarget>().transform;
            //     DataRenderer.GetKDTreeComponent().controllerTransform = cameraTarget;
            // }
            // else
            // {
            //     // VR Controller...
            // }
        }

        // === Noise ===
        public void SetNoise(float value = 0f)
        {
            AstrovisioDataSetRenderer astrovisioDataSetRenderer = DataRenderer.GetAstrovidioDataSetRenderer();
            astrovisioDataSetRenderer.SetNoise(value == 0f ? false : true, value);
        }

        public float GetNoise()
        {
            AstrovisioDataSetRenderer astrovisioDataSetRenderer = DataRenderer.GetAstrovidioDataSetRenderer();
            return astrovisioDataSetRenderer.GetNoiseValue();
        }

        public bool GetNoiseState()
        {
            AstrovisioDataSetRenderer astrovisioDataSetRenderer = DataRenderer.GetAstrovidioDataSetRenderer();
            return astrovisioDataSetRenderer.GetNoiseState();
        }

    }

}
