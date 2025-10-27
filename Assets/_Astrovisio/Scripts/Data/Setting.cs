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
using Newtonsoft.Json;

namespace Astrovisio
{
    public class Setting
    {

        private string name;
        private double thrMin;
        private double thrMax;
        private double? thrMinSel;
        private double? thrMaxSel;
        private string scaling;
        private string mapping;
        private string colormap;
        private string opacity;
        private bool invertMapping;


        [JsonProperty("var_name")]
        public string Name
        {
            get => name;
            set => name = value;
        }

        [JsonProperty("vis_thr_min")]
        public double ThrMin
        {
            get => thrMin;
            set => thrMin = value;
        }

        [JsonProperty("vis_thr_min_sel")]
        public double? ThrMinSel
        {
            get => thrMinSel;
            set => thrMinSel = value;
        }

        [JsonProperty("vis_thr_max")]
        public double ThrMax
        {
            get => thrMax;
            set => thrMax = value;
        }

        [JsonProperty("vis_thr_max_sel")]
        public double? ThrMaxSel
        {
            get => thrMaxSel;
            set => thrMaxSel = value;
        }

        [JsonProperty("scaling")]
        public string Scaling
        {
            get => scaling;
            set => scaling = value;
        }

        [JsonProperty("mapping")]
        public string Mapping
        {
            get => mapping;
            set => mapping = value;
        }

        [JsonProperty("colormap")]
        public string Colormap
        {
            get => colormap;
            set => colormap = value;
        }

        [JsonProperty("opacity")]
        public string Opacity
        {
            get => opacity;
            set => opacity = value;
        }

        [JsonProperty("invert_mapping")]
        public bool InvertMapping
        {
            get => invertMapping;
            set => invertMapping = value;
        }

        internal Setting Clone()
        {
            return new Setting
            {
                Name = Name,
                ThrMin = ThrMin,
                ThrMax = ThrMax,
                ThrMinSel = ThrMinSel,
                ThrMaxSel = ThrMaxSel,
                Scaling = Scaling,
                Mapping = Mapping,
                Colormap = Colormap,
                Opacity = Opacity,
                InvertMapping = InvertMapping
            };
        }

        internal void UpdateFrom(Setting setting)
        {
            Name = setting.Name;
            ThrMin = setting.ThrMin;
            ThrMax = setting.ThrMax;
            ThrMinSel = setting.ThrMinSel;
            ThrMaxSel = setting.ThrMaxSel;
            Scaling = setting.Scaling;
            Mapping = setting.Mapping;
            Colormap = setting.Colormap;
            Opacity = setting.Opacity;
            InvertMapping = setting.InvertMapping;
        }


    }

}
