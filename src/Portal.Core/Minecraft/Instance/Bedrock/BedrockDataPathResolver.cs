using Portal.Bedrock.Standard.Manifest;
using Portal.Core.Minecraft.Classes;

namespace Portal.Core.Minecraft.Instance.Bedrock;

public static class BedrockDataPathResolver
{
    public static string GetDataRoot(BedrockInstanceConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (config.EnableIndependentInstance)
            return config.ShareDataWithOtherLaunchers
                ? Path.Combine(config.InstancePath, "Minecraft Bedrock")
                : GetPortalIsolationRoot(config.InstancePath);

        return GetPortalDataRoot();
    }

    private static string GetPortalIsolationRoot(string instancePath) =>
        Path.Combine(instancePath, "config", "Portal.Desktop", "isolation");

    public static string GetMojangDataRoot(BedrockInstanceConfig config)
    {
        var root = GetDataRoot(config);
        return Path.Combine(root, "Users", "Shared", "games", "com.mojang");
    }

    public static void EnsurePortalDataDirectories()
    {
        var mojangRoot = Path.Combine(GetPortalDataRoot(), "Users", "Shared", "games", "com.mojang");
        foreach (var folder in new[] { "behavior_packs", "minecraftpe", "minecraftWorlds", "resource_packs", "Screenshots", "skin_packs" })
            Directory.CreateDirectory(Path.Combine(mojangRoot, folder));
    }

    private static string GetPortalDataRoot() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "xyz.tiouo.Portal", "Bedrock");

    public static string GetFolder(BedrockInstanceConfig config, MinecraftSpecialFolder folder) => folder switch
    {
        MinecraftSpecialFolder.InstanceFolder => config.InstancePath,
        MinecraftSpecialFolder.SavesFolder => Path.Combine(GetMojangDataRoot(config), "minecraftWorlds"),
        MinecraftSpecialFolder.ResourcePacksFolder => Path.Combine(GetMojangDataRoot(config), "resource_packs"),
        MinecraftSpecialFolder.BehaviorPacksFolder => Path.Combine(GetMojangDataRoot(config), "behavior_packs"),
        MinecraftSpecialFolder.SkinPacksFolder => Path.Combine(GetMojangDataRoot(config), "skin_packs"),
        MinecraftSpecialFolder.WorldTemplatesFolder => Path.Combine(GetMojangDataRoot(config), "world_templates"),
        MinecraftSpecialFolder.DevelopmentResourcePacksFolder => Path.Combine(GetMojangDataRoot(config), "development_resource_packs"),
        MinecraftSpecialFolder.DevelopmentBehaviorPacksFolder => Path.Combine(GetMojangDataRoot(config), "development_behavior_packs"),
        MinecraftSpecialFolder.ScreenshotsFolder => Path.Combine(GetMojangDataRoot(config), "Screenshots"),
        MinecraftSpecialFolder.ConfigFolder => Path.Combine(config.InstancePath, BedrockHelper.ConfigFolder),
        _ => config.InstancePath
    };
}
