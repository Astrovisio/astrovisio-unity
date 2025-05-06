using System.ComponentModel;
using Newtonsoft.Json;

namespace Astrovisio
{
    public class ConfigParam : INotifyPropertyChanged
    {
        private float thrMin;
        private float? thrMinSel;
        private float thrMax;
        private float? thrMaxSel;
        private bool selected;
        private string unit;
        private bool xAxis;
        private bool yAxis;
        private bool zAxis;
        private string[] files;

        [JsonProperty("thr_min")]
        public float ThrMin
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
        public float? ThrMinSel
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
        public float ThrMax
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
        public float? ThrMaxSel
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
