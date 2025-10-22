using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;


namespace Astrovisio
{
    public class File
    {
        private string type;
        private string path;
        private bool processed;
        private long? totalPoints;
        private long? processedPoints;
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

        public long ProcessedPoints
        {
            get
            {
                if (processedPoints.HasValue)
                {
                    return processedPoints.Value;
                }
                return 0;
            }
            set
            {
                if (processedPoints != value)
                {
                    processedPoints = value;
                }
            }
        }

        [JsonProperty("total_points")]
        public long TotalPoints
        {
            get
            {
                if (totalPoints.HasValue)
                {
                    return totalPoints.Value;
                }
                return 0;
            }
            set
            {
                if (totalPoints != value)
                {
                    totalPoints = value;
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

        public Variable GetVariable(string varName)
        {
            return Variables.FirstOrDefault(v => v.Name == varName);
        }

        public Variable GetAxisVariable(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return Variables.FirstOrDefault(v => v.XAxis == true);
                case Axis.Y:
                    return Variables.FirstOrDefault(v => v.YAxis == true);
                case Axis.Z:
                    return Variables.FirstOrDefault(v => v.ZAxis == true);
                default:
                    return null;
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


            if (other.Variables == null)
            {
                if (Variables != null && Variables.Count > 0)
                {
                    Variables.Clear();
                }
                return;
            }

            if (Variables == null)
            {
                Variables = new List<Variable>();
            }

            // --- Removal pass ---
            // Iterate backwards so we can safely remove items by index.
            // For each local variable, check if it still exists in 'other' by Name; if not, remove it.
            for (int i = Variables.Count - 1; i >= 0; i--)
            {
                Variable currentVar = Variables[i];

                if (currentVar == null)
                {
                    Variables.RemoveAt(i);
                    continue;
                }

                bool existsInOther = false;

                foreach (Variable ov in other.Variables)
                {

                    if (ov != null && ov.Name == currentVar.Name)
                    {
                        existsInOther = true;
                        break;
                    }
                }

                if (!existsInOther)
                {
                    Variables.RemoveAt(i);
                }
            }


            foreach (Variable otherVar in other.Variables)
            {
                if (otherVar == null)
                {
                    continue;
                }

                bool found = false;

                for (int i = 0; i < Variables.Count; i++)
                {
                    Variable current = Variables[i];
                    if (current != null && current.Name == otherVar.Name)
                    {
                        // Keep the same reference and just update its fields.
                        current.UpdateFrom(otherVar);
                        found = true;
                        break;
                    }
                }

                // If not found, this is a new variable -> instantiate and copy data.
                if (!found)
                {
                    Variable newVar = new Variable();
                    newVar.UpdateFrom(otherVar);
                    Variables.Add(newVar);
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
