using System.Text.Json.Serialization;

namespace Portal.Core.Minecraft.Instance.Manifest.Bedrock;

public class BedrockInstanceConfig
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("version")] public string Version { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("buildType")] public BedrockBuildType BuildType { get; set; }
    [JsonPropertyName("type")] public InstanceReleaseType Type { get; set; }
}