using CommunityToolkit.Mvvm.ComponentModel;
using MinecraftLaunch.Base.Models.Game;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using Avalonia.Media.Imaging;
using MinecraftLaunch.Base.Enums;
using Portal.Bedrock.Standard.Manifest;
using Portal.Core.Minecraft.Instance.Bedrock;

namespace Portal.Core.Minecraft.Classes;

public class MinecraftInstance : ObservableObject
{
    public MinecraftInstanceType Type { get; init; }

    public MinecraftEntry? MinecraftEntry { get; init; }

    public BedrockInstanceConfig? BedrockConfig { get; init; }

    public string FolderName { get; init; }
    public string FolderPath { get; init; }

    public string InstanceFolderPath { get; init; }

    public DateTime LastPlayTime => Config?.LastPlayTime ?? DateTime.MinValue;

    [JsonIgnore]
    public string DisplayLastPlayTime
    {
        get
        {
            var time = LastPlayTime;
            if (time == DateTime.MinValue)
                return "从未游玩";

            var timeSpan = DateTime.Now - time;

            if (timeSpan.TotalMinutes < 1)
                return "刚刚";

            if (!(timeSpan.TotalDays <= 30)) return time.ToString("yyyy-MM-dd HH:mm");
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} 天前";

            return timeSpan.TotalHours >= 1 ? $"{(int)timeSpan.TotalHours} 小时前" : $"{(int)timeSpan.TotalMinutes} 分钟前";
        }
    }

    public string MinecraftPath
    {
        get
        {
            if (Type == MinecraftInstanceType.Java && MinecraftEntry != null)
                return Path.GetDirectoryName(MinecraftEntry.ClientJarPath);
            return InstanceFolderPath;
        }
    }

    public string InstanceName
    {
        get
        {
            if (Type == MinecraftInstanceType.Java && MinecraftEntry != null)
                return MinecraftEntry.Id;
            if (Type == MinecraftInstanceType.Bedrock && BedrockConfig != null)
                return BedrockConfig.Name;
            return string.Empty;
        }
    }

    public string VersionId
    {
        get
        {
            if (Type == MinecraftInstanceType.Java && MinecraftEntry != null)
                return MinecraftEntry.Version.VersionId;
            if (Type == MinecraftInstanceType.Bedrock && BedrockConfig != null)
                return BedrockConfig.Version;
            return string.Empty;
        }
    }

    public bool IsVanilla
    {
        get
        {
            if (Type == MinecraftInstanceType.Java && MinecraftEntry != null)
                return MinecraftEntry.IsVanilla;
            return false;
        }
    }

    public MinecraftInstanceConfig Config => field ??= GetInstanceConfig();

    public Bitmap Icon => field ??= GetInstanceIcon();

    public string LoaderDescription
    {
        get
        {
            if (Type == MinecraftInstanceType.Java && MinecraftEntry != null)
            {
                return MinecraftEntry.IsVanilla
                    ? "原版"
                    : string.Join(", ", (MinecraftEntry as ModifiedMinecraftEntry)?
                        .ModLoaders.Select(x => x.Type.ToString()) ?? []);
            }
            if (Type == MinecraftInstanceType.Bedrock)
            {
                return "基岩版";
            }
            return string.Empty;
        }
    }

    public string ShortDisplay => $"{LoaderDescription}·{VersionId}";

    public MinecraftInstance(MinecraftEntry e)
    {
        Type = MinecraftInstanceType.Java;
        MinecraftEntry = e;
        InstanceFolderPath = Path.GetDirectoryName(e.ClientJarPath);
    }

    public string Description
    {
        get
        {
            if (Type == MinecraftInstanceType.Bedrock && BedrockConfig != null)
                return BedrockConfig.Description ?? string.Empty;
            return Config?.Note ?? string.Empty;
        }
    }

    public string VersionType
    {
        get
        {
            if (Type == MinecraftInstanceType.Java && MinecraftEntry != null)
                return MinecraftEntry.Version.Type.ToString();
            if (Type == MinecraftInstanceType.Bedrock && BedrockConfig != null)
                return BedrockConfig.Type.ToString();
            return string.Empty;
        }
    }

    public MinecraftInstance(BedrockInstanceConfig bedrockConfig, string folderName, string folderPath)
    {
        Type = MinecraftInstanceType.Bedrock;
        BedrockConfig = bedrockConfig;
        FolderName = folderName;
        FolderPath = folderPath;
        InstanceFolderPath = bedrockConfig.InstancePath;
    }

    private MinecraftInstanceConfig GetInstanceConfig()
    {
        var configPath = Path.Combine(MinecraftPath, "Portal.config.json");
        if (File.Exists(configPath))
            return JsonConvert.DeserializeObject<MinecraftInstanceConfig>(File.ReadAllText(configPath));

        var config = new MinecraftInstanceConfig();
        File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
        return config;
    }

    public void SaveConfig()
    {
        var configPath = Path.Combine(MinecraftPath, "Portal.config.json");
        File.WriteAllText(configPath, JsonConvert.SerializeObject(Config, Formatting.Indented));
    }

    public string GetSpecialFolder(MinecraftSpecialFolder folder)
    {
        if (Type == MinecraftInstanceType.Java && MinecraftEntry != null)
        {
            var basePath = Path.Combine(MinecraftEntry.MinecraftFolderPath, "versions", MinecraftEntry.Id);
            var path = folder switch
            {
                MinecraftSpecialFolder.InstanceFolder => basePath,
                MinecraftSpecialFolder.ModsFolder => Path.Combine(basePath, "mods"),
                MinecraftSpecialFolder.ResourcePacksFolder => Path.Combine(basePath, "resourcepacks"),
                MinecraftSpecialFolder.SavesFolder => Path.Combine(basePath, "saves"),
                MinecraftSpecialFolder.ScreenshotsFolder => Path.Combine(basePath, "screenshots"),
                MinecraftSpecialFolder.ShaderPacksFolder => Path.Combine(basePath, "shaderpacks"),
                _ => basePath
            };

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        return InstanceFolderPath;
    }

    private Bitmap GetInstanceIcon()
    {
        var instanceFolder = GetSpecialFolder(MinecraftSpecialFolder.InstanceFolder);
        var customIcon = Path.Combine(instanceFolder, "icon.png");
        if (File.Exists(customIcon))
        {
            using var s = File.OpenRead(customIcon);
            return Bitmap.DecodeToWidth(s, 48);
        }

        if (Type == MinecraftInstanceType.Bedrock)
        {
            return LoadBitmapFromAssembly("grass_block_side.png");
        }

        var pclIcon = Path.Combine(instanceFolder, "PCL", "Logo.png");
        if (File.Exists(pclIcon))
        {
            using var s = File.OpenRead(pclIcon);
            return Bitmap.DecodeToWidth(s, 48);
        }

        var iconName = GetEmbeddedIconName();
        return LoadBitmapFromAssembly(iconName);
    }

    private string GetEmbeddedIconName()
    {
        if (Type == MinecraftInstanceType.Bedrock)
        {
            return "grass_block_side.png";
        }

        if (MinecraftEntry == null) return "grass_block_side.png";

        if (MinecraftEntry.IsVanilla)
        {
            return MinecraftEntry.Version.Type switch
            {
                MinecraftVersionType.Snapshot => "crafting_table_front.png",
                _ => "grass_block_side.png"
            };
        }

        if (MinecraftEntry is ModifiedMinecraftEntry e && e.ModLoaders != null)
        {
            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Forge)) return "ForgeIcon.png";
            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.NeoForge)) return "NeoForgeIcon.png";
            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Fabric)) return "FabricIcon.png";
            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Quilt)) return "QuiltIcon.png";
            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.OptiFine)) return "OptiFineIcon.png";
        }

        return "grass_block_side.png";
    }

    private static Bitmap LoadBitmapFromAssembly(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourcePath = $"Portal.Core.Assets.McIcons.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
        {
            var defaultPath = "Portal.Core.Assts.McIcons.grass_block_side.png";
            using var defaultStream = assembly.GetManifestResourceStream(defaultPath);
            return defaultStream != null ? Bitmap.DecodeToWidth(defaultStream, 48) : null;
        }

        return Bitmap.DecodeToWidth(stream, 48);
    }
}

public partial class MinecraftInstanceConfig : ObservableObject
{
    [ObservableProperty] public partial string Note { get; set; }
    [ObservableProperty] public partial bool IsFavorite { get; set; }
    [ObservableProperty] public partial bool EnableIndependentInstance { get; set; } = true;
    [ObservableProperty] public partial DateTime LastPlayTime { get; set; } = DateTime.MinValue;
}

public enum MinecraftInstanceType
{
    Java,
    Bedrock
}