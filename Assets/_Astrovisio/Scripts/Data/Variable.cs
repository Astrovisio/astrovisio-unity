using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{
    public class Variable
    {
        private string name;
        private double thrMin;
        private double thrMax;
        private double? thrMinSel;
        private double? thrMaxSel;
        private bool selected;
        private string unit;
        private bool xAxis;
        private bool yAxis;
        private bool zAxis;

        [JsonProperty("var_name")]
        public string Name
        {
            get => name;
            set => name = value;
        }

        [JsonProperty("thr_min")]
        public double ThrMin
        {
            get => thrMin;
            set => thrMin = value;
        }

        [JsonProperty("thr_min_sel")]
        public double? ThrMinSel
        {
            get => thrMinSel;
            set => thrMinSel = value;
        }

        [JsonProperty("thr_max")]
        public double ThrMax
        {
            get => thrMax;
            set => thrMax = value;
        }

        [JsonProperty("thr_max_sel")]
        public double? ThrMaxSel
        {
            get => thrMaxSel;
            set => thrMaxSel = value;
        }

        [JsonProperty("selected")]
        public bool Selected
        {
            get => selected;
            set
            {
                if (selected != value)
                {
                    selected = value;
                }
            }
        }

        [JsonProperty("unit")]
        public string Unit
        {
            get => unit;
            set => unit = value;
        }

        [JsonProperty("x_axis")]
        public bool XAxis
        {
            get => xAxis;
            set
            {
                if (xAxis != value)
                {
                    xAxis = value;
                }
            }
        }

        [JsonProperty("y_axis")]
        public bool YAxis
        {
            get => yAxis;
            set
            {
                if (yAxis != value)
                {
                    yAxis = value;
                }
            }
        }

        [JsonProperty("z_axis")]
        public bool ZAxis
        {
            get => zAxis;
            set
            {
                if (zAxis != value)
                {
                    zAxis = value;
                }
            }
        }

        public void UpdateFrom(Variable other)
        {
            Name = other.name;
            ThrMin = other.ThrMin;
            ThrMinSel = other.ThrMinSel;
            ThrMax = other.ThrMax;
            ThrMaxSel = other.ThrMaxSel;
            Selected = other.Selected;
            Unit = other.Unit;
            XAxis = other.XAxis;
            YAxis = other.YAxis;
            ZAxis = other.ZAxis;
        }

        public Variable DeepCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Variable>(json);
        }

    }
    
}
