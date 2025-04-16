using Newtonsoft.Json;
using System.Collections.Generic;

namespace Astrovisio
{
    public class ConfigProcess
    {
        [JsonProperty("downsampling")]
        public float Downsampling { get; set; }

        // La proprietà "variables" è un dizionario in cui la chiave è una stringa (come "additionalProp1")
        // e il valore è un oggetto del tipo ConfigVariable.
        [JsonProperty("variables")]
        public Dictionary<string, ConfigVariable> Variables { get; set; }
        
    }
}
