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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class ParamRow
    {
        public VisualElement VisualElement { get; set; }
        public ParamRowSettingsController ParamRowSettingsController { get; set; }
    }

    public class ProjectSidebarRenderController
    {

        // === Dependencies ===
        public ProjectSidebarController ProjectSidebarController { private set; get; }
        public UIManager UIManager { private set; get; }
        public ProjectManager ProjectManager { private set; get; }
        public UIContextSO UIContextSO { private set; get; }
        public Project Project { private set; get; }
        public VisualElement Root { private set; get; }

        // === UI ===
        private VisualElement renderSettingsContainer;
        private Button prevReelButton;
        private Button nextReelButton;
        private Label reelLabel;
        private Button xButton;
        private Button yButton;
        private Button zButton;
        private ScrollView paramScrollView;
        private VisualElement settingsPanel;

        // === Axes events ===
        private EventCallback<ClickEvent> xParamClickHandler;
        private EventCallback<ClickEvent> yParamClickHandler;
        private EventCallback<ClickEvent> zParamClickHandler;

        // === Data ===
        private SettingsPanelController settingsPanelController;
        private File currentFile;
        private Settings settings;


        public ProjectSidebarRenderController(
            ProjectSidebarController projectSidebarController,
            UIManager uiManager,
            ProjectManager projectManager,
            UIContextSO uiContextSO,
            Project project,
            VisualElement root)
        {
            ProjectSidebarController = projectSidebarController;
            UIManager = uiManager;
            ProjectManager = projectManager;
            UIContextSO = uiContextSO;
            Project = project;
            Root = root;


            Init();

            ProjectManager.ProjectUpdated += OnProjectUpdated;
            ProjectManager.FileProcessed += OnFileProcessed;
        }

        public void Dispose()
        {
            ProjectManager.ProjectUpdated -= OnProjectUpdated;
            ProjectManager.FileProcessed -= OnFileProcessed;
        }

        private void Init()
        {
            // UpdateCurrentFile();
            // SettingsManager.Instance.TryGetSettings(Project.Id, currentFile.Id, out settings);

            renderSettingsContainer = Root.Q<VisualElement>("RenderSettingsContainer");
            prevReelButton = renderSettingsContainer.Q<Button>("PrevReelButton");
            nextReelButton = renderSettingsContainer.Q<Button>("NextReelButton");
            reelLabel = renderSettingsContainer.Q<Label>("ReelLabel");

            // PREV
            prevReelButton.clicked += () =>
            {
                RenderManager.Instance.RenderReelPrev(Project.Id);
                UpdateCurrentFile();
                UpdateSidebar();
                SettingsManager.Instance.SetSettings(Project.Id, currentFile.Id);
            };

            // NEXT
            nextReelButton.clicked += () =>
            {
                RenderManager.Instance.RenderReelNext(Project.Id);
                UpdateCurrentFile();
                UpdateSidebar();
                SettingsManager.Instance.SetSettings(Project.Id, currentFile.Id);
            };


            VisualElement axisContainer = renderSettingsContainer.Q<VisualElement>("AxisContainer");
            xButton = axisContainer.Q<VisualElement>("XLabel")?.Q<Button>("Root");
            yButton = axisContainer.Q<VisualElement>("YLabel")?.Q<Button>("Root");
            zButton = axisContainer.Q<VisualElement>("ZLabel")?.Q<Button>("Root");

            paramScrollView = renderSettingsContainer.Q<ScrollView>("ParamSettingsScrollView");

            settingsPanel = renderSettingsContainer.Q<VisualElement>("SettingsPanel");
            settingsPanelController = new SettingsPanelController(
                Project,
                settingsPanel,
                UIContextSO,
                OnApplySetting,
                OnCancelSetting
                );

            // settingsPanelController.OnApplyAxisSetting += OnApplyAxisSettings;
            // settingsPanelController.OnApplyParamSetting += OnApplyParamSettings;
            // settingsPanelController.OnCancelSetting += OnCancelSettings;
        }

        private async Task OnApplySetting(Setting setting)
        {
            Debug.Log("Apply clicked");

            SettingsManager.Instance.AddSetting(Project.Id, currentFile.Id, setting);
            await SettingsManager.Instance.UpdateSettings(Project.Id, currentFile.Id);
            SettingsManager.Instance.SetSettings(Project.Id, currentFile.Id);

            UnselectAllButtons();
            CloseSettingsPanel();
            UpdateMappingIcons();
        }

        private void OnCancelSetting()
        {
            Debug.Log("Cancel clicked");

            if (settingsPanelController.GetSettingMode() == SettingsPanelController.SettingMode.Axis)
            {
                // Axes
                SettingsManager.Instance.SetSettings(Project.Id, currentFile.Id);
            }
            else
            {
                // Params
                SettingsManager.Instance.SetSettings(Project.Id, currentFile.Id);
            }

            UnselectAllButtons();
            CloseSettingsPanel();
            UpdateMappingIcons();
        }

        public void Render()
        {
            // Debug.Log($"[Sidebar] Render clicked for Project id={Project?.Id}, name='{Project?.Name}'");

            if (Project?.Files == null)
            {
                Debug.LogWarning("[Sidebar] Project.Files is null");
                return;
            }
            // Debug.Log($"[Sidebar] Files count = {Project.Files.Count}");

            // // Check for data container (debug)
            // foreach (File f in Project.Files.OrderBy(f => f.Order))
            // {
            // bool hasDC = RenderManager.Instance.TryGetDataContainer(Project, f, out var _);
            // Debug.Log($"[Sidebar] File id={f.Id}, name='{f.Name}', processed={f.Processed}, order={f.Order}, processedPath='{f.ProcessedPath}', hasDataContainer={hasDC}");
            // }

            File fileToRender = Project.Files.FirstOrDefault(f => f.Processed);

            if (fileToRender == null)
            {
                fileToRender = Project.Files.FirstOrDefault(f => RenderManager.Instance.TryGetDataContainer(Project, f, out var _));
                if (fileToRender != null)
                {
                    Debug.Log($"[Sidebar] No file with Processed=true found, but a DataContainer for '{fileToRender.Name}' was found. Using it as fallback.");
                }
            }

            if (fileToRender != null)
            {
                // Debug.LogError($"[Sidebar] Rendering file id={fileToRender.Id}, name='{fileToRender.Name}'");
                RenderManager.Instance.RenderReelCurrent(Project.Id);
                UpdateCurrentFile();
                UpdateSidebar();
                SettingsManager.Instance.SetSettings(Project.Id, currentFile.Id);
            }
            else
            {
                Debug.LogWarning($"[Sidebar] No processed file found and no DataContainer registered for Project id={Project.Id}, name='{Project.Name}'. See log above.");
            }

            // UpdateSidebar();
        }

        private void UpdateCurrentFile()
        {
            // Debug.LogWarning("Updating current file...");
            int? fileId = RenderManager.Instance.GetReelCurrentFileId(Project.Id);
            if (fileId == null)
            {
                return;
            }
            currentFile = Project.Files?.FirstOrDefault(f => f.Id == fileId.Value);
            SettingsManager.Instance.TryGetSettings(Project.Id, currentFile.Id, out settings);
            // Debug.Log(settings);
            // Debug.LogError($"Updated current file: {currentFile != null}.");
        }

        public void UpdateSidebar()
        {
            // paramSettingsDatas.Clear();
            UpdateReelLabel();
            UpdateAxesButtons();
            UpdateParamButtons();
            UnselectAllButtons();
            UpdateMappingIcons();
            // CloseSettingsPanel();
        }

        private void UnselectAllButtons()
        {
            xButton.RemoveFromClassList("active");
            yButton.RemoveFromClassList("active");
            zButton.RemoveFromClassList("active");

            foreach (VisualElement paramSettingButton in paramScrollView.Children())
            {
                Button paramButton = paramSettingButton.Q<Button>();
                paramButton.RemoveFromClassList("active");
                paramSettingButton.RemoveFromClassList("active");
                // settingsPanel.RemoveFromClassList("active");
            }
        }

        private void CloseSettingsPanel()
        {
            settingsPanel.RemoveFromClassList("active");
            settingsPanelController.CloseSettingsPanel();
        }

        private void UpdateMappingIcons()
        {
            if (!SettingsManager.Instance.TryGetSettings(Project.Id, currentFile.Id, out var settings) ||
                settings?.Variables == null ||
                settings.Variables.Count == 0 ||
                paramScrollView == null)
            {
                return;
            }

            // Lookup
            Dictionary<string, Button> lookup = new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);
            foreach (var ve in paramScrollView.Children())
            {
                Button btn = ve?.Q<Button>();
                if (btn == null) continue;
                Label lbl = btn.Q<Label>();
                string text = lbl?.text;
                if (string.IsNullOrWhiteSpace(text)) continue;
                if (!lookup.ContainsKey(text)) lookup[text] = btn;
            }

            foreach (Setting setting in settings.Variables)
            {
                if (setting?.Name == null) continue;
                if (!lookup.TryGetValue(setting.Name, out var paramButton) || paramButton == null) continue;

                paramButton.RemoveFromClassList("colormap");
                paramButton.RemoveFromClassList("opacity");

                string mapping = setting.Mapping ?? "None";
                if (string.Equals(mapping, "Opacity", StringComparison.OrdinalIgnoreCase))
                {
                    paramButton.AddToClassList("opacity");
                }
                else if (string.Equals(mapping, "Colormap", StringComparison.OrdinalIgnoreCase))
                {
                    paramButton.AddToClassList("colormap");
                }
                // else: None/unknown -> leave clean
            }
        }

        private void UpdateReelLabel()
        {
            if (currentFile == null)
            {
                reelLabel.text = "";
                return;
            }

            reelLabel.text = currentFile.Name;
        }

        private void UpdateAxesButtons()
        {
            if (currentFile == null)
            {
                return;
            }

            // Update UI
            Label xLabel = xButton.Q<Label>("ParamLabel");
            Label yLabel = yButton.Q<Label>("ParamLabel");
            Label zLabel = zButton.Q<Label>("ParamLabel");

            xLabel.text = currentFile?.Variables.FirstOrDefault(v => v.XAxis)?.Name ?? "";
            yLabel.text = currentFile?.Variables.FirstOrDefault(v => v.YAxis)?.Name ?? "";
            zLabel.text = currentFile?.Variables.FirstOrDefault(v => v.ZAxis)?.Name ?? "";

            VisualElement axisContainer = renderSettingsContainer.Q<VisualElement>("AxisContainer");
            VisualElement xVisualElement = axisContainer.Q<VisualElement>("XLabel");
            VisualElement yVisualElement = axisContainer.Q<VisualElement>("YLabel");
            VisualElement zVisualElement = axisContainer.Q<VisualElement>("ZLabel");

            foreach (Variable variable in currentFile.Variables)
            {
                if (!variable.Selected || (!variable.XAxis && !variable.YAxis && !variable.ZAxis))
                {
                    continue;
                }

                // Debug.LogWarning($"B {variable.Name} - {variable.XAxis} {variable.YAxis} {variable.ZAxis}");

                VisualElement axisVisualElement = null;
                Axis axis;
                if (variable.XAxis)
                {
                    axis = Axis.X;
                    axisVisualElement = xVisualElement;
                }
                else if (variable.YAxis)
                {
                    axis = Axis.Y;
                    axisVisualElement = yVisualElement;
                }
                else if (variable.ZAxis)
                {
                    axis = Axis.Z;
                    axisVisualElement = zVisualElement;
                }
                else
                {
                    continue;
                }

                if (axisVisualElement != null)
                {
                    // Debug.Log("Axis " + variable.Name);

                    // Unregister
                    Button axisButton;
                    if (variable.XAxis)
                    {
                        axisButton = xButton;
                        // Debug.Log(xButton);
                        xButton.RemoveFromClassList("active");
                        if (xParamClickHandler != null)
                        {
                            xButton.UnregisterCallback(xParamClickHandler);
                        }
                    }
                    else if (variable.YAxis)
                    {
                        axisButton = yButton;
                        yButton.RemoveFromClassList("active");
                        if (yParamClickHandler != null)
                        {
                            yButton.UnregisterCallback(yParamClickHandler);
                        }
                    }
                    else if (variable.ZAxis)
                    {
                        axisButton = zButton;
                        zButton.RemoveFromClassList("active");
                        if (zParamClickHandler != null)
                        {
                            zButton.UnregisterCallback(zParamClickHandler);
                        }
                    }
                    else
                    {
                        Debug.LogError("???");
                        return;
                    }

                    // Define click handler
                    EventCallback<ClickEvent> clickHandler = evt =>
                    {
                        Debug.Log("Clicked: " + variable.Name);

                        if (axisButton.ClassListContains("active"))
                        {
                            UnselectAllButtons();
                            CloseSettingsPanel();
                        }
                        else
                        {
                            UnselectAllButtons();
                            axisButton.AddToClassList("active");
                            settingsPanel.AddToClassList("active");

                            Setting setting = SettingsManager.Instance.GetSetting(Project.Id, currentFile.Id, variable.Name);
                            Debug.LogWarning($"Name: {setting.Name} - Mapping: {setting.Mapping} - ThrMin: {setting.ThrMin} - ThrMax: {setting.ThrMax}");
                            settingsPanelController.InitAxisSettingsPanel(currentFile, axis, setting);
                        }
                    };

                    // Save click handler
                    if (variable.XAxis)
                    {
                        xParamClickHandler = clickHandler;
                    }
                    else if (variable.YAxis)
                    {
                        yParamClickHandler = clickHandler;
                    }
                    else if (variable.ZAxis)
                    {
                        zParamClickHandler = clickHandler;
                    }

                    // Register callback
                    axisButton.RegisterCallback(clickHandler);
                }
            }
        }

        private void UpdateParamButtons()
        {
            if (currentFile == null)
            {
                return;
            }

            paramScrollView.Clear();

            foreach (Variable variable in currentFile.Variables)
            {
                if (!variable.Selected || variable.XAxis || variable.YAxis || variable.ZAxis)
                {
                    continue;
                }

                VisualElement paramRowSettings = UIContextSO.paramRowSettingsTemplate.CloneTree();
                paramRowSettings.style.marginBottom = 8;

                Label nameLabel = paramRowSettings.Q<Label>("ParamLabel");
                if (nameLabel != null)
                {
                    nameLabel.text = variable.Name;
                }

                ParamRow paramRow = new ParamRow
                {
                    VisualElement = paramRowSettings,
                    ParamRowSettingsController = new ParamRowSettingsController(variable)
                };

                Button paramButton = paramRowSettings.Q<Button>("Root");
                paramButton.RemoveFromClassList("active");
                paramButton.clicked += () =>
                {
                    Debug.Log("Clicked: " + variable.Name);

                    if (paramButton.ClassListContains("active"))
                    {
                        UnselectAllButtons();
                        CloseSettingsPanel();
                    }
                    else
                    {
                        UnselectAllButtons();
                        paramButton.AddToClassList("active");
                        settingsPanel.AddToClassList("active");


                        Setting setting = SettingsManager.Instance.GetSetting(Project.Id, currentFile.Id, variable.Name);
                        Debug.LogWarning($"Name: {setting.Name} - Mapping: {setting.Mapping} - ThrMin: {setting.ThrMin} - ThrMax: {setting.ThrMax}");
                        settingsPanelController.InitParamSettingsPanel(currentFile, setting);

                        // UpdateRenderManager();
                    }

                };

                // paramSettingsDatas.Add(variable.Name, paramRow);
                paramScrollView.Add(paramRowSettings);

                UpdateMappingIcons();
            }
        }

        private void OnProjectUpdated(Project project)
        {
            if (project == null || project.Id != Project.Id)
            {
                return;
            }

            Project = project;

            UpdateCurrentFile();

            UpdateReelLabel();
            UpdateAxesButtons();
            UpdateParamButtons();
        }

        private void OnFileProcessed(Project project, File file, DataPack pack)
        {
            if (project == null || Project.Id != project.Id)
            {
                return;
            }

            Project = project;

            UpdateCurrentFile();

            UpdateReelLabel();
            UpdateAxesButtons();
            UpdateParamButtons();
        }

    }

}
