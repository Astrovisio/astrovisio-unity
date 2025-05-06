using Newtonsoft.Json;


namespace Astrovisio
{
  public class CreateProjectRequest
  {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("favourite")]
    public bool Favourite { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("paths")]
    public string[] Paths { get; set; }
  }
}
