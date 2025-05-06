using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace Astrovisio
{
    public class ConfigProcess : INotifyPropertyChanged
    {

        [JsonProperty("downsampling")]
        public float Downsampling
        {
            get => downsampling;
            set
            {
                if (downsampling != value)
                {
                    downsampling = value;
                    OnPropertyChanged(nameof(Downsampling));
                }
            }
        }

        private float downsampling;


        [JsonProperty("variables")]
        public Dictionary<string, ConfigParam> Params
        {
            get => parameters;
            set
            {
                if (parameters != value)
                {
                    parameters = value;
                    OnPropertyChanged(nameof(Params));
                }
            }
        }

        private Dictionary<string, ConfigParam> parameters;


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
