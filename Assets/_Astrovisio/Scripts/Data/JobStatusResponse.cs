using Newtonsoft.Json;

public class JobStatusResponse
{
    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("progress")]
    public float Progress { get; set; }

    [JsonProperty("error")]
    public string Error { get; set; }
}
