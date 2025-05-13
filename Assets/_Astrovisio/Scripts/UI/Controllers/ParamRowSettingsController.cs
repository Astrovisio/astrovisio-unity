using UnityEngine;

namespace Astrovisio
{
    public class ParamRowSettingsController
    {
        public Project Project { get; }
        public string ParamName { get; }
        public ParamSettings ParamSettings { get; set; }
        public bool State { get; set; }

        // Local
        private ColorMapSO colorMapSO;

        public ParamRowSettingsController(Project project, string paramName, SideContextSO sideContextSO)
        {
            Project = project;
            ParamName = paramName;
            colorMapSO = sideContextSO.colorMapSO;

            State = false;

            ParamSettings = new ParamSettings();
            ParamSettings.Colormap = "Inferno";
            ParamSettings.Contrast = 1.0f;
            ParamSettings.Saturation = 1.0f;
            ParamSettings.Opacity = 1.0f;
            ParamSettings.Brightness = 1.0f;
            if (project.ConfigProcess.Params.TryGetValue(paramName, out var configParam))
            {
                ParamSettings.MinThreshold = configParam.ThrMinSel ?? configParam.ThrMin;
                ParamSettings.MaxThreshold = configParam.ThrMaxSel ?? configParam.ThrMax;
            }
            else
            {
                Debug.LogWarning($"Param '{paramName}' not found on dictionary.");
            }
        }

    }

}