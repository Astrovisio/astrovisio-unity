using System;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{
    public class ProjectFile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            // Debug.Log($"OnPropertyChanged fired: {propertyName} on {this}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private string name;
        private string path;
        private ConfigProcess configProcess;


        [JsonProperty("paths")]
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        [JsonProperty("paths")]
        public string Path
        {
            get => path;
            set
            {
                if (path != value)
                {
                    path = value;
                    OnPropertyChanged(nameof(Path));
                }
            }
        }

        [JsonProperty("config_process")]
        public ConfigProcess ConfigProcess
        {
            get => configProcess;
            set
            {
                if (configProcess != value)
                {
                    configProcess = value;
                    OnPropertyChanged(nameof(ConfigProcess));
                }
            }
        }

        public void UpdateFrom(ProjectFile other)
        {
            Name = other.Name;
            Path = other.Path;
            ConfigProcess = other.ConfigProcess;
        }

        public ConfigParam DeepCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<ConfigParam>(json);
        }

    }
}