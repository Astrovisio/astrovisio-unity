using System.Collections.Generic;
using Newtonsoft.Json;

namespace Astrovisio
{
    public class Settings
    {

        private List<Setting> variables;

        [JsonProperty("variables")]
        public List<Setting> Variables
        {
            get => variables;
            set => variables = value;
        }

        public void SetDefaults()
        {
            foreach (Setting setting in Variables)
            {
                setting.ThrMinSel = setting.ThrMin;
                setting.ThrMaxSel = setting.ThrMax;
                setting.Mapping = null;
                setting.Colormap = null;
                setting.Scaling = "Linear";
                setting.InvertMapping = false;
            }
        }

    }

}
