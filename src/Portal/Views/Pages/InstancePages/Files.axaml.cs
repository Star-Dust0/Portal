using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Portal.Core.Minecraft.Classes;
using Tio.Avalonia.Standard.Modules.DiskIO;
using Tio.Avalonia.Standard.Modules.Extensions;
using TioUi.Common.Extensions;

namespace Portal.Views.Pages.InstancePages;

public partial class Files : UserControl
{
    public MinecraftInstance Instance { get; }
    public ObservableCollection<InstanceFolderItem> Folders { get; } = [];

    public Files(MinecraftInstance instance)
    {
        Instance = instance;
        foreach (var (name, folder) in new[]
                 {
                     ("模组", MinecraftSpecialFolder.ModsFolder),
                     ("存档", MinecraftSpecialFolder.SavesFolder),
                     ("资源包", MinecraftSpecialFolder.ResourcePacksFolder),
                     ("光影包", MinecraftSpecialFolder.ShaderPacksFolder),
                     ("截图", MinecraftSpecialFolder.ScreenshotsFolder),
                 })
            Folders.Add(new InstanceFolderItem(name, Instance.GetSpecialFolder(folder)));

        InitializeComponent();
        DataContext = this;
    }

    private void OpenInstanceFolder_Click(object? sender, RoutedEventArgs e) => OpenPath(sender as Control, Instance.InstanceFolderPath);

    private void OpenFolder_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { Tag: InstanceFolderItem item } control)
            OpenPath(control, item.Path);
    }

    private static void OpenPath(Control? control, string path)
    {
        if (control != null)
            _ = control.GetTopLevel().Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(path));
    }
}

public record InstanceFolderItem(string Name, string Path);
