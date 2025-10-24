using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Astrovisio.XR
{
    public class XRMenuPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI infoTMP;
        [SerializeField] private Button exitVRButton;
        [SerializeField] private GameObject visual;

        [Header("Settings")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settingsCanvas;

        [Header("Inspector")]
        [SerializeField] private Button dataInspectorButton;
        [SerializeField] private GameObject dataInspectorCanvas;

        [Header("Noise")]
        [SerializeField] private Button noiseButton;
        [SerializeField] private GameObject NoiseCanvas;
        [SerializeField] private Image noiseIcon;
        [SerializeField] private Color activeNoiseIconColor;
        private bool noiseActive;

        [Header("Reels")]
        [SerializeField] private Button reelsButton;
        [SerializeField] private GameObject reelsCanvas;

        [Header("Recorder")]
        [SerializeField] private Button screenRecorderButton;

        [Header("Screenshot")]
        [SerializeField] private Button screenshotButton;

        [Header("Axis")]
        [SerializeField] private Button axisButton;
        [SerializeField] private Image axisIcon;
        [SerializeField] private Color activeAxisIconColor;
        private bool axisVisibility;

        private void Start()
        {
            exitVRButton.onClick.AddListener(HandleExitVRButtonClick);

            settingsButton.onClick.AddListener(HandleSettingsButtonClick);
            dataInspectorButton.onClick.AddListener(HandleDataInspectorButtonClick);
            noiseButton.onClick.AddListener(HandleNoiseButtonClick);
            screenRecorderButton.onClick.AddListener(HandleScreenRecorderButtonClick);
            reelsButton.onClick.AddListener(HandleReelsButtonClick);
            screenshotButton.onClick.AddListener(HandleScreenshotButtonClick);
            axisButton.onClick.AddListener(HandleAxisButtonClick);

            AddHover(settingsButton, () => OnHoverEnter("Settings"), () => OnHoverExit());
            AddHover(dataInspectorButton, () => OnHoverEnter("Inspector"), () => OnHoverExit());
            AddHover(noiseButton, () => OnHoverEnter("Noise"), () => OnHoverExit());
            AddHover(screenRecorderButton, () => OnHoverEnter("Screen recorder"), () => OnHoverExit());
            AddHover(reelsButton, () => OnHoverEnter("Reels"), () => OnHoverExit());
            AddHover(screenshotButton, () => OnHoverEnter("Screenshot"), () => OnHoverExit());
            AddHover(axisButton, () => OnHoverEnter("Axis gizmo"), () => OnHoverExit());

            RenderManager.Instance.OnFileRenderEnd += OnFileRenderEnd;
        }

        private void Update()
        {
            UpdateUI();
        }

        private void OnFileRenderEnd(Project project, File file)
        {
            UpdateUI();
        }

        private void OnDestroy()
        {
            exitVRButton.onClick.RemoveListener(HandleExitVRButtonClick);

            settingsButton.onClick.RemoveListener(HandleSettingsButtonClick);
            dataInspectorButton.onClick.RemoveListener(HandleDataInspectorButtonClick);
            noiseButton.onClick.RemoveListener(HandleNoiseButtonClick);
            screenRecorderButton.onClick.RemoveListener(HandleScreenRecorderButtonClick);
            reelsButton.onClick.RemoveListener(HandleReelsButtonClick);
            screenshotButton.onClick.RemoveListener(HandleScreenshotButtonClick);
            axisButton.onClick.RemoveListener(HandleAxisButtonClick);

            RenderManager.Instance.OnFileRenderEnd -= OnFileRenderEnd;
        }

        private void UpdateUI()
        {
            noiseActive = RenderManager.Instance.GetNoise() > 0 && RenderManager.Instance.GetNoiseState();
            noiseIcon.color = noiseActive ? activeNoiseIconColor : Color.white;

            axisVisibility = SceneManager.Instance.GetAxisGizmoVisibility();
            axisIcon.color = axisVisibility ? activeAxisIconColor : Color.white;
        }

        // === Handlers ===
        private void HandleExitVRButtonClick()
        {
            XRManager.Instance.ExitVR();
        }

        private void HandleSettingsButtonClick()
        {
            // Debug.Log("HandleSettingsButtonClick");
            OpenOrReplacePanel<XRSettingsPanel>(settingsCanvas);
        }

        private void HandleDataInspectorButtonClick()
        {
            // Debug.Log("HandleDataInspectorButtonClick");
            OpenOrReplacePanel<XRDataInspectorPanel>(dataInspectorCanvas);
        }

        private void HandleNoiseButtonClick()
        {
            // Debug.Log("HandleNoiseButtonClick");
            OpenOrReplacePanel<XRNoisePanel>(NoiseCanvas);
        }

        private void HandleReelsButtonClick()
        {
            // Debug.Log("HandleReelsButtonClick");
            OpenOrReplacePanel<XRReelPanel>(reelsCanvas);
        }

        private void HandleScreenRecorderButtonClick()
        {
            Debug.Log("HandleScreenRecorderButtonClick");
        }

        private void HandleScreenshotButtonClick()
        {
            Debug.Log("HandleScreenshotButtonClick");

            // Project currentProject = FindObjectsByType<ProjectManager>()?.GetCurrentProject();
            // Settings settings = SettingsManager.Instance.GetCurrentFileSettings();
            // File file = projectManager.GetCurrentProject().Files.Find(i => i.Id == ReelManager.Instance.GetReelCurrentFileId(projectManager.GetCurrentProject().Id));
            // settings.Path = file.Path;

            // await ScreenshotUtils.TakeScreenshotWithJson(currentProject.Name, file, Camera.main, renderManager.DataRenderer.GetAstrovidioDataSetRenderer().gameObject, settings, uiVisibility);
        }

        private void HandleAxisButtonClick()
        {
            // Debug.Log("HandleAxisButtonClick");
            axisVisibility = SceneManager.Instance.GetAxisGizmoVisibility();
            axisVisibility = !axisVisibility;
            SceneManager.Instance.SetAxesGizmoVisibility(axisVisibility);
            UpdateUI();
        }

        // === Utils ===
        public void TogglePanel()
        {
            if (visual.activeSelf)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }

        public void OpenPanel()
        {
            UpdateUI();
            visual.SetActive(true);
        }

        public void ClosePanel()
        {
            visual.SetActive(false);
        }

        private void AddHover(Button button, Action onEnter, Action onExit)
        {
            var trigger = button.GetComponent<EventTrigger>();
            if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

            // PointerEnter
            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(_ => onEnter());
            trigger.triggers.Add(enterEntry);

            // PointerExit
            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => onExit());
            trigger.triggers.Add(exitEntry);
        }

        private void OnHoverEnter(string info)
        {
            infoTMP.text = info;
        }

        private void OnHoverExit()
        {
            infoTMP.text = "";
        }

        private void OpenOrReplacePanel<TPanel>(GameObject prefab) where TPanel : Component
        {
            TPanel existing = FindAnyObjectByType<TPanel>();
            if (existing != null)
            {
                Canvas topCanvas = existing.GetComponentInParent<Canvas>();

                if (topCanvas != null)
                {
                    Destroy(topCanvas.transform.parent.gameObject);
                }
            }

            if (prefab == null)
            {
                Debug.LogWarning($"Prefab for {typeof(TPanel).Name} is not assigned.");
                return;
            }

            if (XRManager.Instance != null)
            {
                XRManager.Instance.InstantiatePanel(prefab);
            }
            else
            {
                Debug.LogWarning("XRManager.Instance is null. Falling back to plain Instantiate.");
                Instantiate(prefab);
            }
        }

    }

}
