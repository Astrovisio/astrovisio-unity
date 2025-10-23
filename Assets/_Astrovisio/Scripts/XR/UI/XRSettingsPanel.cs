using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using static Astrovisio.SettingsPanelController;

namespace Astrovisio
{
    public class XRSettingsPanel : MonoBehaviour
    {
        [SerializeField] private ParamButton xAxisButton;
        [SerializeField] private ParamButton yAxisButton;
        [SerializeField] private ParamButton zAxisButton;
        [SerializeField] private ScrollRect paramScrollRect;
        [SerializeField] private ParamButton paramButtonPrefab;
        [SerializeField] private XRSettingPanel xrSettingPanel;
        [SerializeField] private Button closeButton;


        private Project currentProject;
        private File currentFile;
        private Setting currentSetting;

        private void Start()
        {
            closeButton.onClick.AddListener(HandleCloseButton);

            RenderManager.Instance.OnFileRenderEnd += OnFileRenderEnd;

            SetSettingPanelVisibility(false);
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(HandleCloseButton);
            RenderManager.Instance.OnFileRenderEnd -= OnFileRenderEnd;
        }

        private void HandleCloseButton()
        {
            Destroy(transform.parent.parent.gameObject, 0.1f);
        }

        private void OnFileRenderEnd(Project project, File file)
        {
            currentProject = project;
            currentFile = file;
            UpdateUI();
        }

        [ContextMenu("UpdateUI")]
        public void UpdateUI()
        {
            ClearScrollView();

            Project project = RenderManager.Instance.renderedProject;
            File file = RenderManager.Instance.renderedFile;
            if (project == null || file == null || paramScrollRect == null || paramScrollRect.content == null || paramButtonPrefab == null)
            {
                return;
            }

            if (!SettingsManager.Instance.TryGetSettings(project.Id, file.Id, out var settings) || settings?.Variables == null)
            {
                return;
            }


            string xAxisVarName = file.GetAxisVariable(Axis.X).Name;
            string yAxisVarName = file.GetAxisVariable(Axis.Y).Name;
            string zAxisVarName = file.GetAxisVariable(Axis.Z).Name;

            xAxisButton.name = xAxisVarName;
            yAxisButton.name = yAxisVarName;
            zAxisButton.name = zAxisVarName;

            Dictionary<string, Axis> axisByVar = new Dictionary<string, Axis>
            {
                { xAxisVarName, Axis.X },
                { yAxisVarName, Axis.Y },
                { zAxisVarName, Axis.Z }
            };

            Dictionary<string, ParamButton> buttonByVar = new Dictionary<string, ParamButton>
            {
                { xAxisVarName, xAxisButton },
                { yAxisVarName, yAxisButton },
                { zAxisVarName, zAxisButton }
            };

            foreach (Setting setting in settings.Variables)
            {
                if (buttonByVar.TryGetValue(setting.Name, out var axisButton))
                {
                    var axis = axisByVar[setting.Name];
                    axisButton.InitButtonSetting(
                        setting.Name,
                        () =>
                        {
                            Debug.Log($"[Axis] - {setting.Name}");
                            if (axisButton.GetButtonState())
                            {
                                UnselectAllButtons();
                                SetSettingPanelVisibility(false);
                                UpdateMappingIcons();
                            }
                            else
                            {
                                UnselectAllButtons();
                                SetSettingPanelVisibility(true);
                                axisButton.SetButtonState(true);
                                Setting extractedSetting = SettingsManager.Instance.GetSetting(currentProject.Id, currentFile.Id, setting.Name);
                                xrSettingPanel?.InitAxisSettingsPanel(project, file, axis, extractedSetting);
                                xrSettingPanel?.SetOnApplyAction(OnApplySetting());
                                xrSettingPanel?.SetOnCancelAction(OnCancelSetting());
                                UpdateMappingIcons();
                                currentSetting = setting;
                            }
                        }
                    );
                    continue;
                }

                ParamButton paramButton = Instantiate(paramButtonPrefab, paramScrollRect.content);
                paramButton.gameObject.name = setting.Name;
                paramButton.gameObject.SetActive(true);
                paramButton.InitButtonSetting(
                    setting.Name,
                    () =>
                    {
                        Debug.Log($"[Param] - {setting.Name} - {paramButton.GetButtonState()}");
                        if (paramButton.GetButtonState())
                        {
                            UnselectAllButtons();
                            SetSettingPanelVisibility(false);
                            UpdateMappingIcons();
                        }
                        else
                        {
                            UnselectAllButtons();
                            SetSettingPanelVisibility(true);
                            paramButton.SetButtonState(true);
                            Setting extractedSetting = SettingsManager.Instance.GetSetting(currentProject.Id, currentFile.Id, setting.Name);
                            xrSettingPanel.InitParamSettingsPanel(project, file, extractedSetting);
                            xrSettingPanel?.SetOnApplyAction(OnApplySetting());
                            xrSettingPanel?.SetOnCancelAction(OnCancelSetting());
                            currentSetting = setting;
                            UpdateMappingIcons();
                        }
                    }
                );
            }

            UpdateMappingIcons();
        }

        private UnityAction OnApplySetting()
        {
            return async () =>
            {
                try
                {
                    Debug.Log("[XRSettingsPanel] Apply clicked");

                    // SettingMode mode = xrSettingPanel.GetSettingMode();

                    Setting tempSetting = xrSettingPanel.GetTempSetting();
                    if (currentProject == null || currentFile == null || tempSetting == null)
                    {
                        Debug.LogWarning("[XRSettingsPanel] Apply aborted: missing project/file/setting.");
                        return;
                    }

                    SettingsManager.Instance.AddSetting(currentProject.Id, currentFile.Id, tempSetting);
                    await SettingsManager.Instance.UpdateSettings(currentProject.Id, currentFile.Id);
                    SettingsManager.Instance.SetSettings(currentProject.Id, currentFile.Id);

                    UnselectAllButtons();
                    SetSettingPanelVisibility(false);
                    UpdateMappingIcons();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[XRSettingsPanel] Apply exception: {ex}");
                }
            };
        }


        private UnityAction OnCancelSetting()
        {
            return () =>
            {
                try
                {
                    Debug.Log("Cancel clicked");

                    if (currentProject == null || currentFile == null)
                    {
                        Debug.LogWarning("[XRSettingsPanel] Cancel: missing project/file; closing panel only.");
                        UnselectAllButtons();
                        SetSettingPanelVisibility(false);
                        return;
                    }

                    if (xrSettingPanel.GetSettingMode() == SettingMode.Axis)
                    {
                        SettingsManager.Instance.SetSettings(currentProject.Id, currentFile.Id);
                    }
                    else
                    {
                        SettingsManager.Instance.SetSettings(currentProject.Id, currentFile.Id);
                    }

                    UnselectAllButtons();
                    SetSettingPanelVisibility(false);
                    UpdateMappingIcons();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            };
        }

        private void UpdateMappingIcons()
        {
            if (currentProject == null || currentFile == null || paramScrollRect == null || paramScrollRect.content == null)
                return;

            if (!SettingsManager.Instance.TryGetSettings(currentProject.Id, currentFile.Id, out var settings) ||
                settings?.Variables == null || settings.Variables.Count == 0)
                return;

            // Lookup: Setting.Name -> Mapping ("Opacity" / "Colormap" / null)
            var mapByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in settings.Variables)
            {
                if (s?.Name == null) continue;
                var mapping = s.Mapping;
                mapByName[s.Name] = mapping;
            }

            ParamButton[] paramButtons = paramScrollRect.content.GetComponentsInChildren<ParamButton>(true);
            foreach (ParamButton paramButton in paramButtons)
            {
                if (paramButton == null) continue;

                var key = !string.IsNullOrEmpty(paramButton.Name) ? paramButton.Name : paramButton.gameObject.name;
                if (string.IsNullOrWhiteSpace(key)) continue;

                if (!mapByName.TryGetValue(key, out var mapping) || string.IsNullOrEmpty(mapping))
                {
                    paramButton.SetButtonIcon(null);
                    continue;
                }

                if (string.Equals(mapping, "Opacity", StringComparison.OrdinalIgnoreCase))
                {
                    paramButton.SetButtonIcon("Opacity");
                }
                else if (string.Equals(mapping, "Colormap", StringComparison.OrdinalIgnoreCase))
                {
                    paramButton.SetButtonIcon("Colormap");
                }
                else
                {
                    paramButton.SetButtonIcon(null);
                }
            }
        }

        private void UnselectAllButtons()
        {
            xAxisButton.SetButtonState(false);
            yAxisButton.SetButtonState(false);
            zAxisButton.SetButtonState(false);
            foreach (ParamButton pb in paramScrollRect.content.GetComponentsInChildren<ParamButton>(true))
            {
                pb.SetButtonState(false);
            }
        }

        private void SetSettingPanelVisibility(bool value)
        {
            xrSettingPanel.gameObject.SetActive(value);
        }

        private void ClearScrollView()
        {
            RectTransform content = paramScrollRect?.content;
            if (content == null) return;

            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }

    }

}
