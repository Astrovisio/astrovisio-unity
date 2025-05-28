using UnityEngine;

namespace Astrovisio
{
    public class ParamRowSettingsController
    {
        public Project Project { get; }
        public ConfigParam Param { get; }
        public string ParamName { get; }
        public ParamSettings ParamSettings { get; set; }
        public bool State { get; set; }

        // Local
        private ColorMapSO colorMapSO;

        public ParamRowSettingsController(Project project, ConfigParam param, string paramName, UIContextSO uiContextSO)
        {
            Project = project;
            Param = param;
            ParamName = paramName;
            colorMapSO = uiContextSO.colorMapSO;

            State = false;

            ParamSettings = new ParamSettings();
            ParamSettings.Colormap = "Inferno";


            if (project.ConfigProcess.Params.TryGetValue(paramName, out var configParam))
            {
                ParamSettings.MinThreshold = configParam.ThrMin;
                ParamSettings.MaxThreshold = configParam.ThrMax;
                ParamSettings.MinThresholdSelected = configParam.ThrMinSel ?? configParam.ThrMin;
                ParamSettings.MaxThresholdSelected = configParam.ThrMaxSel ?? configParam.ThrMax;
            }
            else
            {
                Debug.LogWarning($"Param '{paramName}' not found on dictionary.");
            }
        }

    }

}