/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Metaverso SRL
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
