using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;


namespace Astrovisio
{
    public class File : INotifyPropertyChanged
    {
        private string type;
        private string path;
        private bool processed;
        private float downsampling = 1f;
        private string processedPath;
        private int order;
        private string name;
        private long size;
        private int id;
        private List<Variable> variables = new();


        [JsonProperty("type")]
        public string Type
        {
            get => type;
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        [JsonProperty("path")]
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

        [JsonProperty("processed")]
        public bool Processed
        {
            get => processed;
            set
            {
                if (processed != value)
                {
                    processed = value;
                    OnPropertyChanged(nameof(Processed));
                }
            }
        }

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

        [JsonProperty("processed_path")]
        public string ProcessedPath
        {
            get => processedPath;
            set
            {
                if (processedPath != value)
                {
                    processedPath = value;
                    OnPropertyChanged(nameof(ProcessedPath));
                }
            }
        }

        [JsonProperty("order")]
        public int Order
        {
            get => order;
            set
            {
                if (order != value)
                {
                    order = value;
                    OnPropertyChanged(nameof(Order));
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

        [JsonProperty("size")]
        public long Size
        {
            get => size;
            set
            {
                if (size != value)
                {
                    size = value;
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        [JsonProperty("variables")]
        public List<Variable> Variables
        {
            get => variables;
            set
            {
                if (variables != value)
                {
                    variables = value ?? new List<Variable>();
                    OnPropertyChanged(nameof(Variables));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            // Debug.Log("OnPropertyChanged: " + propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateFrom(File other)
        {
            if (other == null)
            {
                return;
            }

            Type = other.Type;
            Path = other.Path;
            Processed = other.Processed;
            Downsampling = other.Downsampling;
            ProcessedPath = other.ProcessedPath;
            Id = other.Id;

            if (other.Variables == null)
            {
                Variables = new List<Variable>();
            }
            else
            {
                List<Variable> copy = JsonConvert.DeserializeObject<List<Variable>>(JsonConvert.SerializeObject(other.Variables));
                Variables = copy ?? new List<Variable>();
            }
        }

        public File DeepCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<File>(json);
        }

    }

}
