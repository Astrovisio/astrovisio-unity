using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio.XR
{
    public class XRMenuPanel : MonoBehaviour
    {
        [SerializeField] private Button exitVRButton;

        [Header("Settings")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settingsCanvas;

        [Header("Inspector")]
        [SerializeField] private Button dataInspectorButton;
        [SerializeField] private GameObject dataInspectorCanvas;

        [Header("Noise")]
        [SerializeField] private Button noiseButton;
        [SerializeField] private GameObject NoiseCanvas;

        [Header("Reels")]
        [SerializeField] private Button reelsButton;
        [SerializeField] private GameObject reelsCanvas;

        [Header("Recorder")]
        [SerializeField] private Button screenRecorderButton;

        [Header("Screenshot")]
        [SerializeField] private Button screenshotButton;

        [Header("Axis")]
        [SerializeField] private Button axisButton;

        private void Start()
        {
            settingsButton.onClick.AddListener(HandleSettingsButtonClick);
            dataInspectorButton.onClick.AddListener(HandleDataInspectorButtonClick);
            noiseButton.onClick.AddListener(HandleNoiseButtonClick);
            screenRecorderButton.onClick.AddListener(HandleScreenRecorderButtonClick);
            reelsButton.onClick.AddListener(HandleReelsButtonClick);
            screenshotButton.onClick.AddListener(HandleScreenshotButtonClick);
            axisButton.onClick.AddListener(HandleAxisButtonClick);
        }

        private void OnDestroy()
        {
            settingsButton.onClick.RemoveListener(HandleSettingsButtonClick);
            dataInspectorButton.onClick.RemoveListener(HandleDataInspectorButtonClick);
            noiseButton.onClick.RemoveListener(HandleNoiseButtonClick);
            screenRecorderButton.onClick.RemoveListener(HandleScreenRecorderButtonClick);
            reelsButton.onClick.RemoveListener(HandleReelsButtonClick);
            screenshotButton.onClick.RemoveListener(HandleScreenshotButtonClick);
            axisButton.onClick.RemoveListener(HandleAxisButtonClick);
        }

        // === Handlers ===
        private void HandleSettingsButtonClick()
        {
            Debug.Log("HandleSettingsButtonClick");
            OpenOrReplacePanel<XRSettingsPanel>(settingsCanvas);
        }

        private void HandleDataInspectorButtonClick()
        {
            Debug.Log("HandleDataInspectorButtonClick");
            OpenOrReplacePanel<XRDataInspectorPanel>(dataInspectorCanvas);
        }

        private void HandleNoiseButtonClick()
        {
            Debug.Log("HandleNoiseButtonClick");
            OpenOrReplacePanel<XRNoisePanel>(NoiseCanvas);
        }

        private void HandleReelsButtonClick()
        {
            Debug.Log("HandleReelsButtonClick");
            OpenOrReplacePanel<XRReelPanel>(reelsCanvas);
        }

        private void HandleScreenRecorderButtonClick()
        {
            Debug.Log("HandleScreenRecorderButtonClick");
        }

        private void HandleScreenshotButtonClick()
        {
            Debug.Log("HandleScreenshotButtonClick");
        }

        private void HandleAxisButtonClick()
        {
            Debug.Log("HandleAxisButtonClick");
        }

        private void OpenOrReplacePanel<TPanel>(GameObject prefab) where TPanel : Component
        {
            var existing = FindAnyObjectByType<TPanel>();
            if (existing != null)
            {
                var topCanvas = existing.GetComponentInParent<Canvas>();
                GameObject toDestroy =
                    topCanvas != null ? topCanvas.gameObject :
                    existing.transform.parent != null ? existing.transform.parent.gameObject :
                    existing.gameObject;

                Destroy(toDestroy);
            }

            if (prefab != null)
            {
                Instantiate(prefab);
            }
            else
            {
                Debug.LogWarning($"Prefab for {typeof(TPanel).Name} is not assigned.");
            }
        }

    }

}
