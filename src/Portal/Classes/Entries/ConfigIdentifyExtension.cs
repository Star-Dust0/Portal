using Portal.Const;
using Portal.Core.Minecraft.Classes;
using Tio.Avalonia.Standard.Modules.DiskIO;

namespace Portal.Classes.Entries;

public class ConfigIdentifyExtension
{
    public static void MinecraftFolder(ConfigEntry entry)
    {
        if (entry.MinecraftFolders.Count == 0)
        {
            Helper.TryCreateFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "portal.minecraft"));
            entry.MinecraftFolders.Add(new MinecraftFolderEntry
            {
                FolderName = "Portal 默认文件夹",
                FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "portal.minecraft")
            });
            entry.DefaultMinecraftFolder = entry.MinecraftFolders[0];
            return;
        }

        if (entry.DefaultMinecraftFolder == null)
        {
            entry.DefaultMinecraftFolder = entry.MinecraftFolders[0];
            return;
        }

        if (!entry.MinecraftFolders.Contains(entry.DefaultMinecraftFolder))
        {
            entry.DefaultMinecraftFolder = entry.MinecraftFolders[0];
        }
    }
}