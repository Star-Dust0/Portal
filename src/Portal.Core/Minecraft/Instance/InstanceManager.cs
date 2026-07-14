using MinecraftLaunch.Components.Parser;
using Portal.Core.Minecraft.Classes;
using Portal.Core.Minecraft.Instance.Bedrock;

namespace Portal.Core.Minecraft.Instance;

public class InstanceManager
{
    private readonly string _gameRootFolder;
    private readonly string _folderName;

    public List<string> VersionFolders { get; } = new() { "versions", "bedrock_versions" };
    
    
    public InstanceManager(string gameRootFolder, string folderName)
    {
        _gameRootFolder = gameRootFolder;
        _folderName = folderName;
    }

    public List<MinecraftInstance> RefreshInstances()
    {
        var instances = new List<MinecraftInstance>();

        MinecraftParser minecraftParser = _gameRootFolder;
        var javaEntries = minecraftParser.GetMinecrafts().ToDictionary(e => e.Id);

        var processedFolders = new HashSet<string>();

        foreach (var versionFolder in VersionFolders)
        {
            var versionsFolderPath = Path.Combine(_gameRootFolder, versionFolder);
            if (!Directory.Exists(versionsFolderPath))
            {
                if (versionFolder == "versions")
                    Directory.CreateDirectory(versionsFolderPath);
                continue;
            }

            foreach (var instanceFolder in Directory.GetDirectories(versionsFolderPath))
            {
                var folderKey = Path.GetFullPath(instanceFolder);
                if (processedFolders.Contains(folderKey))
                    continue;
                processedFolders.Add(folderKey);

                var instanceType = GetInstanceType(instanceFolder);

                if (instanceType == MinecraftInstanceType.Java)
                {
                    var folderName = Path.GetFileName(instanceFolder);
                    if (javaEntries.TryGetValue(folderName, out var minecraftEntry))
                    {
                        instances.Add(new MinecraftInstance(minecraftEntry)
                        {
                            FolderName = _folderName,
                            FolderPath = _gameRootFolder
                        });
                    }
                }
                else if (instanceType == MinecraftInstanceType.Bedrock)
                {
                    try
                    {
                        var bedrockConfig = BedrockHelper.GetInstanceConfig(instanceFolder);
                        instances.Add(new MinecraftInstance(bedrockConfig, _folderName));
                    }
                    catch
                    {
                    }
                }
            }
        }

        return instances;
    }

    public static MinecraftInstanceType GetInstanceType(string instanceFolder)
    {
        if (File.Exists(Path.Combine(instanceFolder, "appxmanifest.xml")))
            return MinecraftInstanceType.Bedrock;
        if (File.Exists(Path.Combine(instanceFolder, $"{Path.GetFileName(instanceFolder)}.json")))
            return MinecraftInstanceType.Java;

        return MinecraftInstanceType.Java;
    }
}