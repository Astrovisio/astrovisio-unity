using Newtonsoft.Json;

namespace Astrovisio
{
    public class ReplaceProjectFilesRequest
    {
        [JsonProperty("paths")]
        public string[] Paths { get; set; }

    }

}
