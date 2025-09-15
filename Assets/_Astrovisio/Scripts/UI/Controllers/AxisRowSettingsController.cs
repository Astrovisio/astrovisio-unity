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
            Variables Variables = project.Files.Params[paramName];

            if (TryGetAxis(Variables, out Axis axis))
            {
                AxisRenderSettings = new AxisRenderSettings(
                    paramName,
                    axis,
                    (float)Variables.ThrMin,
                    (float)Variables.ThrMax,
                    (float)(Variables.ThrMinSel ?? Variables.ThrMin),
                    (float)(Variables.ThrMaxSel ?? Variables.ThrMax),
                    ScalingType.Linear
                );
            }
            else
            {
                AxisRenderSettings = null;
            }
        }

        private bool TryGetAxis(Variables param, out Axis axis)
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
