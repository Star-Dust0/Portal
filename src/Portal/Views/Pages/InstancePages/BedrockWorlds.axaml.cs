using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Portal.Core.Minecraft.Classes;
using Portal.Core.Minecraft.Instance.Bedrock;

namespace Portal.Views.Pages.InstancePages;

public partial class BedrockWorlds : UserControl
{
    private readonly MinecraftInstance? _instance;

    public ObservableCollection<string> WorldUserIds { get; } = [];

    public BedrockWorlds()
    {
        InitializeComponent();
        DataContext = this;
    }

    public BedrockWorlds(MinecraftInstance instance)
    {
        InitializeComponent();
        _instance = instance;
        DataContext = this;
        AttachedToVisualTree += (_, _) => RefreshWorldUserIds();
    }

    private async void OpenFolder_OnClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not { } topLevel || _instance == null)
            return;

        await topLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(GetSelectedWorldsFolder()));
    }

    private void RefreshWorldUserIds()
    {
        if (_instance?.BedrockConfig is not { } config)
            return;

        var selectedUserId = WorldUserIdSelector.SelectedItem as string;
        var userIds = BedrockDataPathResolver.GetWorldUserIds(config);
        WorldUserIds.Clear();
        foreach (var userId in userIds)
            WorldUserIds.Add(userId);

        WorldUserIdSelector.SelectedItem = selectedUserId != null && WorldUserIds.Contains(selectedUserId)
            ? selectedUserId
            : WorldUserIds.FirstOrDefault(userId => !string.Equals(userId, "Shared", StringComparison.OrdinalIgnoreCase))
              ?? WorldUserIds.FirstOrDefault();
    }

    private string GetSelectedWorldsFolder()
    {
        if (_instance?.BedrockConfig is not { } config)
            return string.Empty;

        var userId = WorldUserIdSelector.SelectedItem as string ?? "Shared";
        var path = BedrockDataPathResolver.GetWorldsFolder(config, userId);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return path;
    }

    private void WorldUserIdSelector_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
    }

    private void Title_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
    }
}
