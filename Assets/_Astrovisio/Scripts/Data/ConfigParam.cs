using System.ComponentModel;
using System.Diagnostics;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{
    public class ConfigParam : INotifyPropertyChanged
    {
        private double thrMin;
        private double? thrMinSel;
        private double thrMax;
        private double? thrMaxSel;
        private bool selected;
        private string unit;
        private bool xAxis;
        private bool yAxis;
        private bool zAxis;
        private string[] files;

        [JsonProperty("thr_min")]
        public double ThrMin
        {
            get => thrMin;
            set
            {
                if (thrMin != value)
                {
                    thrMin = value;
                    OnPropertyChanged(nameof(ThrMin));
                }
            }
        }

        [JsonProperty("thr_min_sel")]
        public double? ThrMinSel
        {
            get => thrMinSel;
            set
            {
                if (thrMinSel != value)
                {
                    thrMinSel = value;
                    OnPropertyChanged(nameof(ThrMinSel));
                }
            }
        }

        [JsonProperty("thr_max")]
        public double ThrMax
        {
            get => thrMax;
            set
            {
                if (thrMax != value)
                {
                    thrMax = value;
                    OnPropertyChanged(nameof(ThrMax));
                }
            }
        }

        [JsonProperty("thr_max_sel")]
        public double? ThrMaxSel
        {
            get => thrMaxSel;
            set
            {
                if (thrMaxSel != value)
                {
                    thrMaxSel = value;
                    OnPropertyChanged(nameof(ThrMaxSel));
                }
            }
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
                    OnPropertyChanged(nameof(Selected));
                }
            }
        }

        [JsonProperty("unit")]
        public string Unit
        {
            get => unit;
            set
            {
                if (unit != value)
                {
                    unit = value;
                    OnPropertyChanged(nameof(Unit));
                }
            }
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
                    OnPropertyChanged(nameof(XAxis));
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
                    OnPropertyChanged(nameof(YAxis));
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
                    OnPropertyChanged(nameof(ZAxis));
                }
            }
        }

        [JsonProperty("files")]
        public string[] Files
        {
            get => files;
            set
            {
                if (files != value)
                {
                    files = value;
                    OnPropertyChanged(nameof(Files));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            // Debug.Log($"OnPropertyChanged fired: {propertyName} on {this}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateFrom(ConfigParam other)
        {
            ThrMin = other.ThrMin;
            ThrMinSel = other.ThrMinSel;
            ThrMax = other.ThrMax;
            ThrMaxSel = other.ThrMaxSel;
            Selected = other.Selected;
            Unit = other.Unit;
            XAxis = other.XAxis;
            YAxis = other.YAxis;
            ZAxis = other.ZAxis;

            Files = other.Files != null ? (string[])other.Files.Clone() : null;
        }

        public ConfigParam DeepCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<ConfigParam>(json);
        }

    }
}
