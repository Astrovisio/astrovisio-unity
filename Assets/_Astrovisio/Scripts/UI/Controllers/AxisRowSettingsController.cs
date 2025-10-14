using System;
using CatalogData;

namespace Astrovisio
{
    public class AxisRowSettingsController : AbstractRowSettingsController, ICloneable
    {
        public AxisRenderSettings AxisRenderSettings { get; private set; }

        public AxisRowSettingsController(Variable variable)
            : base(variable)
        {
            Variable Variable = variable;

            if (TryGetAxis(Variable, out Axis axis))
            {
                AxisRenderSettings = new AxisRenderSettings(
                    variable.Name,
                    axis,
                    (float)Variable.ThrMin,
                    (float)Variable.ThrMax,
                    (float)(Variable.ThrMinSel ?? Variable.ThrMin),
                    (float)(Variable.ThrMaxSel ?? Variable.ThrMax),
                    ScalingType.Linear
                );
            }
            else
            {
                AxisRenderSettings = null;
            }
        }

        private bool TryGetAxis(Variable variable, out Axis axis)
        {
            if (variable.XAxis)
            {
                axis = Axis.X;
                return true;
            }
            if (variable.YAxis)
            {
                axis = Axis.Y;
                return true;
            }
            if (variable.ZAxis)
            {
                axis = Axis.Z;
                return true;
            }

            axis = default;
            return false;
        }

        public object Clone()
        {
            return new AxisRowSettingsController(Variable)
            {
                AxisRenderSettings = AxisRenderSettings?.Clone() as AxisRenderSettings
            };
        }

    }

}
