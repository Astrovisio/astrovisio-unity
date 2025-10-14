using System.Collections.Generic;
using CatalogData;
using Newtonsoft.Json;

namespace Astrovisio
{
    public class Settings
    {

        private List<Setting> variables;
        private string path;

        [JsonProperty("variables")]
        public List<Setting> Variables
        {
            get => variables;
            set => variables = value;
        }

        [JsonProperty("path")]
        public string Path
        {
            get => path;
            set => path = value;
        }

        public void SetDefaults(File file)
        {
            variables.Clear();

            foreach (Variable variable in file.Variables)
            {
                if (!variable.Selected)
                {
                    continue;
                }

                Setting setting = new Setting
                {
                    Name = variable.Name,
                    Mapping = null,
                    ThrMin = variable.ThrMin,
                    ThrMax = variable.ThrMax,
                    ThrMinSel = variable.ThrMin,
                    ThrMaxSel = variable.ThrMax,
                    Scaling = ScalingType.Linear.ToString(),
                    Colormap = ColorMapEnum.Autumn.ToString(),
                    InvertMapping = false,
                };

                variables.Add(setting);
            }
        }

    }

}
