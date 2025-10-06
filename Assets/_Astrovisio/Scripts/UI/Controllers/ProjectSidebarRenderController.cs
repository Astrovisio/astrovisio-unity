using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class AxisRow
    {
        public VisualElement VisualElement { get; set; }
        public AxisRowSettingsController AxisRowSettingsController { get; set; }
        public EventCallback<ClickEvent> ClickHandler;
    }

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

        // === Click events ===
        private EventCallback<ClickEvent> xParamClickHandler;
        private EventCallback<ClickEvent> yParamClickHandler;
        private EventCallback<ClickEvent> zParamClickHandler;


        // === Data ===



        private SettingsPanelController settingsPanelController;
        private File currentFile;
        // private Setting currentSetting;
        private Settings settings;

        // private Dictionary<string, AxisRow> axisSettingsData = new();
        // private Dictionary<string, ParamRow> paramSettingsDatas = new();
        // private ProjectRenderSettings projectRenderSettings = new();



        public ProjectSidebarRenderController(
            ProjectSidebarController projectSidebarController,
            UIManager uiManager,
            ProjectManager projectManager,
            UIContextSO uiContextSO,
            Project project,
            VisualElement root
            )
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
            };

            // NEXT
            nextReelButton.clicked += () =>
            {
                RenderManager.Instance.RenderReelNext(Project.Id);
                UpdateCurrentFile();
                UpdateSidebar();
            };


            VisualElement axisContainer = renderSettingsContainer.Q<VisualElement>("AxisContainer");
            xButton = axisContainer.Q<VisualElement>("XLabel")?.Q<Button>("Root");
            yButton = axisContainer.Q<VisualElement>("YLabel")?.Q<Button>("Root");
            zButton = axisContainer.Q<VisualElement>("ZLabel")?.Q<Button>("Root");

            paramScrollView = renderSettingsContainer.Q<ScrollView>("ParamSettingsScrollView");

            settingsPanel = renderSettingsContainer.Q<VisualElement>("SettingsPanel");
            settingsPanelController = new SettingsPanelController(Project, settingsPanel, UIContextSO);

            settingsPanelController.OnApplyAxisSetting += OnApplyAxisSettings;
            settingsPanelController.OnApplyParamSetting += OnApplyParamSettings;
            settingsPanelController.OnCancelSetting += OnCancelSettings;
        }

        public void Dispose()
        {
            ProjectManager.ProjectUpdated -= OnProjectUpdated;
            ProjectManager.FileProcessed -= OnFileProcessed;
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
            }
            else
            {
                Debug.LogWarning($"[Sidebar] No processed file found and no DataContainer registered for Project id={Project.Id}, name='{Project.Name}'. See log above.");
            }
            
            UpdateSidebar();
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
            // Debug.LogError($"Updated current file: {currentFile != null}.");
        }

        public void UpdateSidebar()
        {
            // paramSettingsDatas.Clear();
            UpdateReelLabel();
            UpdateAxesButtons();
            UpdateParamButtons();
            UnselectAllButtons();
            // CloseSettingsPanel();
        }

        private void UnselectAllButtons()
        {
            xButton.RemoveFromClassList("active");
            yButton.RemoveFromClassList("active");
            zButton.RemoveFromClassList("active");

            paramScrollView.contentContainer.Query<Button>().ForEach(button =>
            {
                button.RemoveFromClassList("active");
            });

            settingsPanel.RemoveFromClassList("active");

            // foreach (ParamRow paramSettingsData in paramSettingsDatas.Values)
            // {
            //     Button button = paramSettingsData.VisualElement.Q<Button>("Root");
            //     if (button != null)
            //     {
            //         button.RemoveFromClassList("active");
            //         settingsPanel.RemoveFromClassList("active");
            //     }
            // }
            // CloseSettingsPanel();
        }

        private void CloseSettingsPanel()
        {
            // settingsPanelController.UnregisterSettingPanelEvents();
            settingsPanelController.CloseSettingsPanel();

            foreach (VisualElement paramSettingButton in paramScrollView.Children())
            {
                Button paramButton = paramSettingButton.Q<Button>();
                paramButton.RemoveFromClassList("active");
                paramSettingButton.RemoveFromClassList("active");
                // settingsPanel.RemoveFromClassList("active");
            }
        }

        private void OnApplyAxisSettings(Setting setting)
        {
            string appliedParamName = setting.Name;

            // TODO: API call GB

            CloseSettingsPanel();
            xButton.RemoveFromClassList("active");
            yButton.RemoveFromClassList("active");
            zButton.RemoveFromClassList("active");
        }

        private void OnApplyParamSettings(Setting setting)
        {
            string appliedParamName = setting.Name;
            // MappingType appliedMapping = appliedParamRowSettingsController.ParamRenderSettings.Mapping;

            // Debug.Log("Applied Param Name: " + appliedParamName + " " + appliedMapping);
            // PrintAllMappings();

            // Update project params
            // switch (appliedMapping)
            // {
            //     case MappingType.None:
            //         // Debug.Log("OnApplySettings -> None: " + appliedParamName);
            //         if (
            //             projectRenderSettings.OpacitySettingsController != null &&
            //             projectRenderSettings.OpacitySettingsController.Variable.Name == appliedParamName)
            //         {
            //             // Debug.Log("Resetting opacity...");
            //             projectRenderSettings.OpacitySettingsController.Reset();
            //             projectRenderSettings.OpacitySettingsController = null;
            //         }
            //         else if (
            //             projectRenderSettings.ColorMapSettingsController != null &&
            //             projectRenderSettings.ColorMapSettingsController.Variable.Name == appliedParamName)
            //         {
            //             // Debug.Log("Resetting colormap...");
            //             projectRenderSettings.ColorMapSettingsController.Reset();
            //             projectRenderSettings.ColorMapSettingsController = null;
            //         }
            //         break;
            //     case MappingType.Opacity:
            //         // Debug.Log("OnApplySettings -> Opacity: " + appliedParamName + " " + appliedParamRowSettingsController.RenderSettings.MappingSettings.ScalingType);
            //         ResetParamsByMappingType(appliedParamName, MappingType.Opacity);
            //         projectRenderSettings.OpacitySettingsController = appliedParamRowSettingsController;
            //         break;
            //     case MappingType.Colormap:
            //         // Debug.Log("OnApplySettings -> Colormap: " + appliedParamName);
            //         ResetParamsByMappingType(appliedParamName, MappingType.Colormap);
            //         projectRenderSettings.ColorMapSettingsController = appliedParamRowSettingsController;
            //         break;
            // }

            // SetParamRowSettingsController(appliedParamName, appliedParamRowSettingsController);

            CloseSettingsPanel();
            // UpdateRenderManager();
            UpdateMappingIcons();
            // PrintAllMappings();
        }

        // private void ResetParamsByMappingType(string appliedParamName, MappingType appliedMappingType)
        // {
        //     foreach (var paramSettings in paramSettingsDatas)
        //     {
        //         string paramName = paramSettings.Key;
        //         ParamRow paramRow = paramSettings.Value;
        //         ParamRowSettingsController paramRowSettingsController = paramRow.ParamRowSettingsController;
        //         MappingType paramMappingType = paramRowSettingsController.ParamRenderSettings.Mapping;

        //         // Debug.Log(paramName + " <-> " + appliedParamName);
        //         if (paramName == appliedParamName)
        //         {
        //             if (paramMappingType == MappingType.Opacity)
        //             {
        //                 projectRenderSettings.OpacitySettingsController = null;
        //                 // Debug.Log(paramName + " opacity null");
        //             }
        //             else if (paramMappingType == MappingType.Colormap)
        //             {
        //                 projectRenderSettings.ColorMapSettingsController = null;
        //                 // Debug.Log(paramName + " colormap null");
        //             }
        //         }
        //     }

        //     foreach (var paramSettings in paramSettingsDatas)
        //     {
        //         ParamRow paramRow = paramSettings.Value;
        //         ParamRowSettingsController paramRowSettingsController = paramRow.ParamRowSettingsController;
        //         MappingType paramMappingType = paramRowSettingsController.ParamRenderSettings.Mapping;
        //         if (paramMappingType == appliedMappingType)
        //         {
        //             paramRowSettingsController.Reset();
        //         }
        //     }
        // }

        // private void UpdateRenderManager()
        // {
        //     if (projectRenderSettings.ColorMapSettingsController is null)
        //     {
        //         // Debug.Log("Removing colormap");
        //         RenderManager.Instance.RenderSettingsController.RemoveColorMap();
        //     }
        //     else
        //     {
        //         // Debug.Log("Setting colormap " + projectRenderSettings.ColorMapSettingsController.ParamName);
        //         RenderManager.Instance.RenderSettingsController.SetRenderSettings(projectRenderSettings.ColorMapSettingsController.ParamRenderSettings);
        //     }

        //     if (projectRenderSettings.OpacitySettingsController is null)
        //     {
        //         // Debug.Log("Removing opacity");
        //         RenderManager.Instance.RenderSettingsController.RemoveOpacity();
        //     }
        //     else
        //     {
        //         // Debug.Log("Setting opacity " + projectRenderSettings.OpacitySettingsController.ParamName);
        //         RenderManager.Instance.RenderSettingsController.SetRenderSettings(projectRenderSettings.OpacitySettingsController.ParamRenderSettings);
        //     }
        // }

        private void OnCancelSettings()
        {
            CloseSettingsPanel();
            // UpdateRenderManager();
            UpdateMappingIcons();
            // PrintAllMappings();
        }

        private void UpdateMappingIcons()
        {

            foreach (Setting setting in settings.Variables)
            {

                Button paramButton = null;
                Label paramLabel = null;
                paramScrollView.contentContainer.Query<Button>().ForEach(button =>
                {
                    paramButton = button;
                    Label label = button.Q<Label>();
                    if (label.text == setting.Name)
                    {
                        paramButton = button;
                        paramLabel = label;
                    }
                });


                switch (setting.Mapping)
                {
                    case null:
                        paramButton.RemoveFromClassList("colormap");
                        paramButton.RemoveFromClassList("opacity");
                        continue;
                    case "Opacity":
                        paramButton.RemoveFromClassList("colormap");
                        paramButton.AddToClassList("opacity");
                        break;
                    case "Colormap":
                        paramButton.AddToClassList("colormap");
                        paramButton.RemoveFromClassList("opacity");
                        break;
                    default:
                        paramButton.RemoveFromClassList("colormap");
                        paramButton.RemoveFromClassList("opacity");
                        continue;
                }

                // paramScrollView.contentContainer.Query<Button>().ForEach(button =>
                // {
                //     Label label = button.Q<Label>();
                //     if (label.text == setting.Name)
                //     {

                //     }
                // });

                // string paramName = paramSettings.Key;
                // ParamRow paramRow = paramSettings.Value;

                // VisualElement paramVisualElement = paramRow.VisualElement.Q<VisualElement>("Root");
                // paramVisualElement.RemoveFromClassList("colormap");
                // paramVisualElement.RemoveFromClassList("opacity");
                // paramVisualElement.RemoveFromClassList("haptics");
                // paramVisualElement.RemoveFromClassList("sound");
                // // Debug.Log("Remove all from " + paramName);

                // if (paramName == colormapParamName)
                // {
                //     paramVisualElement.AddToClassList("colormap");
                //     // Debug.Log("Add colormap to " + paramName);
                // }
                // else if (paramName == opacityParamName)
                // {
                //     paramVisualElement.AddToClassList("opacity");
                //     // Debug.Log("Add opacity to " + paramName);
                // }
            }

        }

        // private void PrintAllMappings()
        // {
        //     string opacityParamName = (projectRenderSettings.OpacitySettingsController != null) ? projectRenderSettings.OpacitySettingsController.Variable.Name : "null";
        //     string colormapParamName = (projectRenderSettings.ColorMapSettingsController != null) ? projectRenderSettings.ColorMapSettingsController.Variable.Name : "null";

        //     Debug.Log("PrintAllMapping");
        //     Debug.Log("Opacity: " + opacityParamName);
        //     Debug.Log("Colormap: " + colormapParamName);
        // }

        // private void SetAxisRowSettingsController(string paramName, AxisRowSettingsController axisRowSettingsController)
        // {
        //     if (axisSettingsData.TryGetValue(paramName, out AxisRow axisRow))
        //     {
        //         // Debug.Log($"Set: {axisRowSettingsController.AxisRenderSettings.ThresholdMinSelected} {axisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected}");
        //         axisRow.AxisRowSettingsController = axisRowSettingsController;
        //     }
        // }

        // private void SetParamRowSettingsController(string paramName, ParamRowSettingsController paramRowSettingsController)
        // {
        //     if (paramSettingsDatas.TryGetValue(paramName, out ParamRow paramRow))
        //     {
        //         paramRow.ParamRowSettingsController = paramRowSettingsController;
        //     }
        // }

        // private AxisRowSettingsController GetAxisRowSettingsController(string paramName)
        // {
        //     if (axisSettingsData.TryGetValue(paramName, out AxisRow axisRow))
        //     {
        //         // Debug.Log($"Get: {axisRow.AxisRowSettingsController.AxisRenderSettings.ThresholdMinSelected} {axisRow.AxisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected}");
        //         return axisRow.AxisRowSettingsController;
        //     }

        //     return null;
        // }

        // private ParamRowSettingsController GetParamRowSettingsController(string paramName)
        // {
        //     if (paramSettingsDatas.TryGetValue(paramName, out ParamRow paramRow))
        //     {
        //         // Debug.Log("GetParamRowSettingsController: " + paramName + " -> " + paramRow.ParamRowSettingsController.RenderSettings.Mapping);
        //         return paramRow.ParamRowSettingsController;
        //     }

        //     return null;
        // }

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
            paramScrollView.Clear();
            // axisSettingsData.Clear(); // GB
            // paramSettingsDatas.Clear(); // GB

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
                    // Debug.Log("Clicked: " + variable.Name);

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
