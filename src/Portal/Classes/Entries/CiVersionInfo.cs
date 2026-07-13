using Newtonsoft.Json;

namespace Portal.Classes.Entries;

public class CiVersionInfo
{
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("build-time")]
    public DateTime BuildTime { get; set; }

    [JsonProperty("action")]
    public string Action { get; set; } = string.Empty;

    [JsonProperty("commit")]
    public string Commit { get; set; } = string.Empty;

    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;
}