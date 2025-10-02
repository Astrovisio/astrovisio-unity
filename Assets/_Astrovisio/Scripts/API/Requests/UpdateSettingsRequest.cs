using System.Collections.Generic;
using Newtonsoft.Json;

namespace Astrovisio
{
    public class UpdateSettingsRequest
    {

        [JsonProperty("variables", NullValueHandling = NullValueHandling.Ignore)]
        public List<Setting> Variables { get; set; } = new();

    }

}
