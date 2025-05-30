using System.Linq;
using UnityEngine;

namespace Astrovisio
{

    public class ParamRowSettingsController
    {

        // === Dependencies ===
        public string ParamName { get; }
        public Project Project { get; }

        // === Other ===
        public RenderSettings RenderSettings { get; }


        public ParamRowSettingsController(string paramName, Project project)
        {
            ParamName = paramName;
            Project = project;

            ConfigParam param = Project.ConfigProcess.Params.FirstOrDefault(p => p.Key == paramName).Value;

            RenderSettings = new RenderSettings(
                paramName,
                (float)param.ThrMin,
                (float)param.ThrMax,
                (float)param.ThrMinSel,
                (float)param.ThrMaxSel,
                MappingType.Colormap,
                new ColorMapSettings(ColorMapEnum.Accent, ScalingType.Linear, (float)param.ThrMin, (float)param.ThrMax, false)
            );

            if (Project.ConfigProcess.Params.TryGetValue(paramName, out var configParam))
            {
                RenderSettings.ThresholdMinSelected = (float)configParam.ThrMin;
                RenderSettings.ThresholdMaxSelected = (float)configParam.ThrMax;
                RenderSettings.ThresholdMin = (float)(configParam.ThrMinSel ?? configParam.ThrMin);
                RenderSettings.ThresholdMax = (float)(configParam.ThrMaxSel ?? configParam.ThrMax);
            }
            else
            {
                Debug.LogWarning($"Param '{paramName}' not found on dictionary.");
            }
        }



    }

}