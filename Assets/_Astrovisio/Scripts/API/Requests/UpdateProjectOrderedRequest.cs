using Newtonsoft.Json;

namespace Astrovisio
{
    public class UpdateProjectOrderedRequest : UpdateProjectRequest
    {

        [JsonProperty("order")]
        public int[] Order { get; set; }

    }
    
}
