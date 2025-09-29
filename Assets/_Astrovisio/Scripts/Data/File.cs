using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;


namespace Astrovisio
{
    public class File
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
                }
            }
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
            Order = other.Order;
            Name = other.Name;
            Size = other.Size;
            Id = other.Id;

            if (Variables == null || other.Variables == null)
            {
                return;
            }

            foreach (Variable otherVar in other.Variables)
            {
                if (otherVar == null)
                {
                    continue;
                }

                for (int i = 0; i < Variables.Count; i++)
                {
                    Variable currentVar = Variables[i];
                    if (currentVar != null && string.Equals(currentVar.Name, otherVar.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        currentVar.UpdateFrom(otherVar);
                        break;
                    }
                }
            }
        }


        public File DeepCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<File>(json);
        }

    }

}
