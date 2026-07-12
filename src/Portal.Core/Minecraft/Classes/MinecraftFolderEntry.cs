using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using TioUi.Common.Extensions;

namespace Portal.Core.Minecraft.Classes;

public record MinecraftFolderEntry
{
    public string FolderName { get; set; }
    public string FolderPath { get; set; }
    public bool EnableIndependentVersion { get; set; } = true;
    public void OpenFolder(object parameter)
    {
        (parameter as Control)!.GetTopLevel().Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(FolderPath));
    }
}