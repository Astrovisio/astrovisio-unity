using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Astrovisio
{
    [Serializable]
    public class SavedProject
    {
        private Project project;
        private List<Settings> filesSettings;

        [JsonProperty("project")]
        public Project Project
        {
            get => project;
            set
            {
                if (project != value)
                {
                    project = value;
                }
            }
        }

        [JsonProperty("filesSettings")]
        public List<Settings> FilesSettings
        {
            get => filesSettings;
            set
            {
                if (filesSettings != value)
                {
                    filesSettings = value;
                }
            }
        }
        
        public string[] GetFilePaths()
        {
            List<string> paths = new List<string>();
            foreach (File file in Project.Files)
            {
                paths.Add(file.Path);
            }

            return paths.ToArray();
        }

    }
}
