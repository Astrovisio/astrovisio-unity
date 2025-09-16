using Newtonsoft.Json;

namespace Astrovisio
{
    public class ReplaceProjectFileRequest
    {
        [JsonProperty("paths")]
        public string[] Paths { get; set; }

    }

}
