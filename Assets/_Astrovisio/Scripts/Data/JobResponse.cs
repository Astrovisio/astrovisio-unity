using Newtonsoft.Json;

public class JobResponse
{
    [JsonProperty("job_id")]
    public int JobID { get; set; }
}