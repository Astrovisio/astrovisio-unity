using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

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

        public void UpdateFrom(ConfigProcess other)
        {
            Downsampling = other.Downsampling;

            if (Params == null)
                Params = new Dictionary<string, ConfigParam>();

            foreach (var kvp in other.Params)
            {
                if (Params.ContainsKey(kvp.Key))
                {
                    Params[kvp.Key].UpdateFrom(kvp.Value); // aggiorna i valori
                }
                else
                {
                    Params[kvp.Key] = kvp.Value.DeepCopy(); // nuovo parametro
                }
            }
        }

        public ConfigProcess DeepCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<ConfigProcess>(json);
        }

    }
}
