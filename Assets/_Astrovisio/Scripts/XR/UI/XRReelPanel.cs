using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class XRReelPanel : MonoBehaviour
    {
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI labelTMP;
        private ProjectManager projectManager;

        private void Start()
        {
            projectManager = FindAnyObjectByType<ProjectManager>();

            if (prevButton != null) prevButton.onClick.AddListener(OnPrevClick);
            if (nextButton != null) nextButton.onClick.AddListener(OnNextClick);
        }

        private void OnEnable()
        {
            projectManager = FindAnyObjectByType<ProjectManager>();
        }

        private void OnDestroy()
        {
            if (prevButton != null) prevButton.onClick.RemoveListener(OnPrevClick);
            if (nextButton != null) nextButton.onClick.RemoveListener(OnNextClick);
        }

        [ContextMenu("Update")]
        public void UpdateUI()
        {
            if (projectManager == null || ReelManager.Instance == null)
            {
                if (labelTMP != null)
                {
                    labelTMP.text = "—";
                }
                return;
            }

            Project project = projectManager.GetCurrentProject();
            if (project == null)
            {
                if (labelTMP != null)
                {
                    labelTMP.text = "—";
                }
                return;
            }

            File file = ReelManager.Instance.GetReelCurrentFile(project.Id);
            if (labelTMP != null)
            {
                labelTMP.text = file?.Name ?? "—";
            }
        }

        private Project GetCurrentProjectId()
        {
            if (projectManager == null || ReelManager.Instance == null)
            {
                return null;
            }

            Project project = projectManager.GetCurrentProject();
            if (project == null)
            {
                return null;
            }

            return project;
        }

        private void OnPrevClick()
        {
            Project project = GetCurrentProjectId();
            if (project == null)
            {
                return;
            }

            RenderManager.Instance.RenderReelPrev(project.Id);
            int? currentFileId = RenderManager.Instance.GetReelCurrentFileId(project.Id);
            if (currentFileId.HasValue == false)
            {
                return;
            }

            SettingsManager.Instance.SetSettings(project.Id, currentFileId.Value);
            UpdateUI();
        }

        private void OnNextClick()
        {
            Project project = GetCurrentProjectId();
            if (project == null)
            {
                return;
            }

            RenderManager.Instance.RenderReelNext(project.Id);
            int? currentFileId = RenderManager.Instance.GetReelCurrentFileId(project.Id);
            if (currentFileId.HasValue == false)
            {
                return;
            }

            SettingsManager.Instance.SetSettings(project.Id, currentFileId.Value);
            UpdateUI();
        }

    }

}
