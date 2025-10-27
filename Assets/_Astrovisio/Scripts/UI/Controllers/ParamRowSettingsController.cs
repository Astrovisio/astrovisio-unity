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

namespace Astrovisio
{
    public class ParamRowSettingsController : AbstractRowSettingsController, ICloneable
    {
        public ParamRenderSettings ParamRenderSettings { get; private set; }

        public ParamRowSettingsController(Variable variable)
            : base(variable)
        {
            ParamRenderSettings = new ParamRenderSettings(variable.Name, MappingType.None);
        }

        public object Clone()
        {
            return new ParamRowSettingsController(Variable)
            {
                ParamRenderSettings = ParamRenderSettings?.Clone() as ParamRenderSettings
            };
        }

        public void Reset()
        {
            ParamRenderSettings = new ParamRenderSettings(Variable.Name, MappingType.None);
        }
        
    }

}
