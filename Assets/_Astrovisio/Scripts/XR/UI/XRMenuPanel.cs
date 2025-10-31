/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Alkemy, Metaverso
 *
 * This file is part of the Astrovisio project.
 *
 * Astrovisio is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Astrovisio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Astrovisio.XR
{
    public class XRMenuPanel : MonoBehaviour
    {
        [SerializeField] private XRScreenshotUIController xrScreenshotUIController;
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
        [SerializeField] private Image screenRecorderIcon;

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
            // Debug.Log("HandleScreenRecorderButtonClick");

            if (RecorderManager.Instance.IsRecording == false)
            {
                screenRecorderIcon.color = activeNoiseIconColor;
                RecorderManager.Instance.StartRecording(false);
            }
            else
            {
                RecorderManager.Instance.StopRecording();
                screenRecorderIcon.color = Color.white;
            }
        }

        private void HandleScreenshotButtonClick()
        {
            // Debug.Log("HandleScreenshotButtonClick");
            _ = TakeScreenshot();
        }

        public async Task TakeScreenshot(bool uiVisibility = false)
        {
            Project currentProject = RenderManager.Instance.renderedProject;
            File currentFile = RenderManager.Instance.renderedFile;
            Settings settings = SettingsManager.Instance.GetCurrentFileSettings();
            settings.Path = currentFile.Path;

            xrScreenshotUIController.SetLabel("TAKING SCREENSHOT...");
            await RunTimerAsync(3f);

            await ScreenshotUtils.TakeScreenshotWithJson(
                currentProject.Name,
                currentFile,
                Camera.main,
                RenderManager.Instance.DataRenderer.GetAstrovidioDataSetRenderer().gameObject,
                settings,
                uiVisibility,
                false);

            xrScreenshotUIController.SetLabel("DONE");
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
        public void DestroyAllPanels()
        {
            XRDataInspectorPanel xrDataInspectorPanel = FindAnyObjectByType<XRDataInspectorPanel>();
            if (xrDataInspectorPanel != null)
            {
                // Debug.LogWarning("xrDataInspectorPanel");
                Destroy(xrDataInspectorPanel.GetComponentInParent<Canvas>().transform.parent.gameObject);
            }

            XRNoisePanel xrNoisePanel = FindAnyObjectByType<XRNoisePanel>();
            if (xrNoisePanel != null)
            {
                // Debug.LogWarning("xrNoisePanel");
                Destroy(xrNoisePanel.GetComponentInParent<Canvas>().transform.parent.gameObject);
            }

            XRReelPanel xrReelPanel = FindAnyObjectByType<XRReelPanel>();
            if (xrReelPanel != null)
            {
                // Debug.LogWarning("xrReelPanel");
                Destroy(xrReelPanel.GetComponentInParent<Canvas>().transform.parent.gameObject);
            }

            XRSettingsPanel xrSettingsPanel = FindAnyObjectByType<XRSettingsPanel>();
            if (xrSettingsPanel != null)
            {
                // Debug.LogWarning("xrSettingsPanel");
                Destroy(xrSettingsPanel.GetComponentInParent<Canvas>().transform.parent.gameObject);
            }
        }

        private async Task RunTimerAsync(float duration)
        {
            xrScreenshotUIController.SetLoaderImage(true, 1f);

            float t = 0f;
            while (t < duration)
            {
                await Task.Yield();

                t += Time.deltaTime;
                float remaining = 1f - Mathf.Clamp01(t / duration);
                xrScreenshotUIController.SetLoaderImage(true, remaining);
            }

            xrScreenshotUIController.SetLoaderImage(false, 0f);
        }

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
