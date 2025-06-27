using System;
using CatalogData;

namespace Astrovisio
{
    public class AxisRowSettingsController : AbstractRowSettingsController, ICloneable
    {
        public AxisRenderSettings AxisRenderSettings { get; private set; }

        public AxisRowSettingsController(string paramName, Project project)
            : base(paramName, project)
        {
            ConfigParam configParam = project.ConfigProcess.Params[paramName];

            if (TryGetAxis(configParam, out Axis axis))
            {
                AxisRenderSettings = new AxisRenderSettings(
                    paramName,
                    axis,
                    (float)configParam.ThrMin,
                    (float)configParam.ThrMax,
                    (float)(configParam.ThrMinSel ?? configParam.ThrMin),
                    (float)(configParam.ThrMaxSel ?? configParam.ThrMax),
                    ScalingType.Linear
                );
            }
            else
            {
                AxisRenderSettings = null;
            }
        }

        private bool TryGetAxis(ConfigParam param, out Axis axis)
        {
            if (param.XAxis)
            {
                axis = Axis.X;
                return true;
            }
            if (param.YAxis)
            {
                axis = Axis.Y;
                return true;
            }
            if (param.ZAxis)
            {
                axis = Axis.Z;
                return true;
            }

            axis = default;
            return false;
        }

        public object Clone()
        {
            return new AxisRowSettingsController(ParamName, Project)
            {
                AxisRenderSettings = AxisRenderSettings?.Clone() as AxisRenderSettings
            };
        }

    }

}
