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

    }

}
