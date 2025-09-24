using System.Collections.Generic;
using Newtonsoft.Json;

namespace Astrovisio
{
    public class UpdateFileRequest
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("processed")]
        public bool Processed { get; set; }

        [JsonProperty("downsampling")]
        public float Downsampling { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("processed_path")]
        public string ProcessedPath { get; set; }

        [JsonProperty("variables")]
        public List<Variable> Variables { get; set; }

    }

}
