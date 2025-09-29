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
            // If 'other' is null, there is nothing to merge/update.
            if (other == null)
            {
                return;
            }

            // Copy scalar/file-level fields from 'other' (shallow copy).
            Type = other.Type;
            Path = other.Path;
            Processed = other.Processed;
            Downsampling = other.Downsampling;
            ProcessedPath = other.ProcessedPath;
            Order = other.Order;
            Name = other.Name;
            Size = other.Size;
            Id = other.Id;

            // --- Variables sync (remove/update/add) ---
            // If the source file has no variables, the canonical state is "no variables".
            // Clear local list (if any) and return.
            if (other.Variables == null)
            {
                if (Variables != null && Variables.Count > 0)
                {
                    Variables.Clear();
                }
                return;
            }

            // Ensure our local list exists before proceeding.
            if (Variables == null)
            {
                Variables = new List<Variable>();
            }

            // --- Removal pass ---
            // Iterate backwards so we can safely remove items by index.
            // For each local variable, check if it still exists in 'other' by Name; if not, remove it.
            for (int i = Variables.Count - 1; i >= 0; i--)
            {
                Variable current = Variables[i];
                // Defensive check: remove null entries if any have crept in.
                if (current == null)
                {
                    Variables.RemoveAt(i);
                    continue;
                }

                bool existsInOther = false;
                // Linear scan of 'other.Variables' to find a matching Name.
                foreach (Variable ov in other.Variables)
                {
                    // Matching key: Variable.Name (adjust here if you later introduce an Id).
                    if (ov != null && ov.Name == current.Name)
                    {
                        existsInOther = true;
                        break;
                    }
                }
                // If not found in 'other', this variable was removed on the source side -> remove locally.
                if (!existsInOther)
                {
                    Variables.RemoveAt(i);
                }
            }

            // --- Upsert pass (update or insert) ---
            // For each variable in 'other':
            //   - If we already have it (same Name), update the existing instance (preserve reference).
            //   - If we don't have it, create a new instance and copy values from 'other'.
            foreach (Variable otherVar in other.Variables)
            {
                if (otherVar == null)
                {
                    continue;
                }

                bool found = false;
                // Look for a local variable with the same Name.
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
