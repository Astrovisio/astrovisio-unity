using System;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{
    public class Project : INotifyPropertyChanged
    {
        private string name;
        private bool favourite;
        private string description;
        private int id;
        private DateTime? created;
        private DateTime? lastOpened;
        private string[] paths;
        private ConfigProcess configProcess;

        [JsonProperty("name")]
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

        [JsonProperty("favourite")]
        public bool Favourite
        {
            get => favourite;
            set
            {
                if (favourite != value)
                {
                    favourite = value;
                    OnPropertyChanged(nameof(Favourite));
                }
            }
        }

        [JsonProperty("description")]
        public string Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        [JsonProperty("id")]
        public int Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        [JsonProperty("created")]
        public DateTime? Created
        {
            get => created;
            set
            {
                if (created != value)
                {
                    created = value;
                    OnPropertyChanged(nameof(Created));
                }
            }
        }

        [JsonProperty("last_opened")]
        public DateTime? LastOpened
        {
            get => lastOpened;
            set
            {
                if (lastOpened != value)
                {
                    lastOpened = value;
                    OnPropertyChanged(nameof(LastOpened));
                }
            }
        }

        [JsonProperty("paths")]
        public string[] Paths
        {
            get => paths;
            set
            {
                if (paths != value)
                {
                    paths = value;
                    OnPropertyChanged(nameof(Paths));
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

        public Project(string name, string description, bool favourite = false, string[] paths = null)
        {
            Name = name;
            Description = description;
            Favourite = favourite;
            Paths = paths ?? Array.Empty<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateFrom(Project other)
        {
            Name = other.Name;
            Description = other.Description;
            Favourite = other.Favourite;
            Created = other.Created;
            LastOpened = other.LastOpened;

            // Copia i valori dei path
            if (other.Paths != null)
                Paths = other.Paths.ToArray();

            // Copia i valori di ConfigProcess, ma senza creare una nuova istanza se non serve
            if (ConfigProcess != null && other.ConfigProcess != null)
            {
                ConfigProcess.UpdateFrom(other.ConfigProcess);
            }
            else if (other.ConfigProcess != null)
            {
                ConfigProcess = other.ConfigProcess.DeepCopy();
            }
        }

        public Project DeepCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            Debug.Log($"DeepCopy of {json}");
            return JsonConvert.DeserializeObject<Project>(json);
        }

        public string Print()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
    
}
