namespace Portal.Core;

public static class ServiceCredentials
{
    public const string CurseForgeApiKeyEnvironmentVariable = "CURSEFORGE_API_KEY";
    public const string MicrosoftClientIdEnvironmentVariable = "MICROSOFT_CLIENT_ID";
    private const string CurseForgeApiKeyMetadataKey = "Portal.CurseForgeApiKey";
    private const string MicrosoftClientIdMetadataKey = "Portal.MicrosoftClientId";

    public static string? CurseForgeApiKey => Get(CurseForgeApiKeyMetadataKey, CurseForgeApiKeyEnvironmentVariable);

    public static string MicrosoftClientId => Get(MicrosoftClientIdMetadataKey, MicrosoftClientIdEnvironmentVariable)
        ?? throw new InvalidOperationException(
            $"Microsoft login requires {MicrosoftClientIdEnvironmentVariable} to be set when building or running Portal.");

    private static string? Get(string metadataKey, string environmentVariable)
    {
        var embedded = typeof(ServiceCredentials).Assembly
            .GetCustomAttributes(false)
            .OfType<System.Reflection.AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == metadataKey)?.Value;
        if (!string.IsNullOrWhiteSpace(embedded))
            return embedded;

        var value = Environment.GetEnvironmentVariable(environmentVariable)?.Trim();
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
