using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AsyncImageLoader;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Portal.Core.Minecraft.Classes;
using Portal.Core.Minecraft.Services;
using Portal.Views.StaticPages;
using Tio.Avalonia.Standard.Modules.DiskIO;
using Tio.Avalonia.Standard.Modules.Extensions;
using Tio.Avalonia.Standard.Tab.Gateway;
using TioUi.Common;
using TioUi.Common.Extensions;
using TioUi.Controls;

namespace Portal.Views.Pages.InstancePages;

public partial class Saves : UserControl, INotifyPropertyChanged
{
    private readonly MinecraftInstance? _instance;
    private readonly string? _savesPath;
    private readonly WorldSaveService _saveService = new();
    private bool _hasLoaded;
    private bool _isLoading;
    private string _filter = string.Empty;

    public ObservableCollection<SaveItem> Items { get; } = [];
    public ObservableCollection<SaveItem> FilteredItems { get; } = [];

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading == value) return;
            _isLoading = value;
            RaisePropertyChanged(nameof(IsLoading));
        }
    }

    public bool IsEmpty => !IsLoading && FilteredItems.Count == 0;
    public string SaveCountText => IsLoading ? string.Empty : $"{FilteredItems.Count} 个";

    public Saves()
    {
        InitializeComponent();
        DataContext = this;
    }

    public Saves(MinecraftInstance instance) : this()
    {
        _instance = instance;
        _savesPath = instance.GetSpecialFolder(MinecraftSpecialFolder.SavesFolder);
        AttachedToVisualTree += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (_hasLoaded || _instance == null)
            return;

        _hasLoaded = true;
        IsLoading = true;
        RaiseListProperties();
        var saves = await _saveService.ScanAsync(_instance);
        Items.Clear();
        foreach (var save in saves)
            Items.Add(new SaveItem(save));
        ApplyFilter();
        IsLoading = false;
        RaiseListProperties();
    }

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(_filter)
            ? Items
            : Items.Where(item => item.FolderName.Contains(_filter, StringComparison.OrdinalIgnoreCase) ||
                                  item.DisplayName.Contains(_filter, StringComparison.OrdinalIgnoreCase));
        FilteredItems.Clear();
        foreach (var item in filtered)
            FilteredItems.Add(item);
        RaiseListProperties();
    }

    private async void OpenFolder_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_savesPath))
            await TopLevel.GetTopLevel(this).Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(_savesPath));
    }

    private async void OpenWorldFolder_OnClick(object? sender, RoutedEventArgs e)
    {
        if (GetItem(sender) is { } item)
            await TopLevel.GetTopLevel(this).Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(item.Info.FolderPath));
    }

    private async void ShowInfo_OnClick(object? sender, RoutedEventArgs e)
    {
        if (GetItem(sender) is not { } item)
            return;
        await OverlayDialog.ShowStandardAsync(
            new TextBlock
                { Margin = new Avalonia.Thickness(24), Text = item.Details, TextWrapping = TextWrapping.Wrap }, null,
            this.TryGetHostId(),
            new OverlayDialogOptions
            {
                Title = item.DisplayName, Mode = DialogMode.None, Buttons = DialogButton.OK, CanLightDismiss = true
            });
    }

    private async void DeleteWorld_OnClick(object? sender, RoutedEventArgs e)
    {
        if (GetItem(sender) is not { } item || !Directory.Exists(item.Info.FolderPath))
            return;
        var result = await OverlayDialog.ShowStandardAsync(
            new TextBlock
            {
                Margin = new Avalonia.Thickness(24), Text = $"确定要永久删除存档“{item.DisplayName}”吗？此操作无法撤销。",
                TextWrapping = TextWrapping.Wrap
            },
            null, this.TryGetHostId(), CreateDeleteConfirmationOptions("删除存档"));
        if (result != DialogResult.Yes)
            return;
        try
        {
            Directory.Delete(item.Info.FolderPath, true);
            Items.Remove(item);
            ApplyFilter();
        }
        catch (IOException)
        {
            await ShowErrorAsync("存档正在被其他程序使用，无法删除。");
        }
        catch (UnauthorizedAccessException)
        {
            await ShowErrorAsync("没有删除此存档的权限。");
        }
    }

    private Task ShowErrorAsync(string message) => OverlayDialog.ShowStandardAsync(
        new TextBlock { Margin = new Avalonia.Thickness(24), Text = message, TextWrapping = TextWrapping.Wrap }, null,
        this.TryGetHostId(),
        new OverlayDialogOptions
            { Title = "存档", Mode = DialogMode.Error, Buttons = DialogButton.OK, CanLightDismiss = false });

    private static OverlayDialogOptions CreateDeleteConfirmationOptions(string title) => new()
    {
        Title = title,
        Mode = DialogMode.Error,
        Buttons = DialogButton.YesNo,
        OverrideYesButtonText = "删除",
        OverrideNoButtonText = "取消",
        CanLightDismiss = false,
        CanResize = false
    };

    private static SaveItem? GetItem(object? sender) => (sender as Control)?.Tag as SaveItem;

    private void SearchBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        _filter = (sender as TextBox)?.Text ?? string.Empty;
        ApplyFilter();
    }

    private void Title_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _hasLoaded = false;
        _ = LoadAsync();
    }

    private void RaiseListProperties()
    {
        RaisePropertyChanged(nameof(IsEmpty));
        RaisePropertyChanged(nameof(SaveCountText));
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
    }
}

public sealed class SaveItem(WorldSaveInfo info)
{
    public WorldSaveInfo Info { get; } = info;
    public string FolderName => Info.FolderName;
    public string DisplayName => string.IsNullOrWhiteSpace(Info.LevelName) ? Info.FolderName : Info.LevelName;
    public string? IconPath => Info.IconPath;
    public bool HasIcon => IconPath != null;
    public IAsyncImageLoader ImageLoader { get; } = new SaveImageLoader();
    public string Summary => $"{Info.Version ?? "未知版本"}·{GetGameModeText(Info.GameMode)}";
    public string LastPlayedText => $"最近游玩：{(Info.LastPlayedTime ?? Info.LastWriteTime):yyyy-MM-dd HH:mm}";

    public string Details =>
        $"文件夹：{Info.FolderName}\n创建时间：{Info.CreationTime:yyyy-MM-dd HH:mm}\n修改时间：{Info.LastWriteTime:yyyy-MM-dd HH:mm}\n最近游玩：{(Info.LastPlayedTime?.ToString("yyyy-MM-dd HH:mm") ?? "未知")}\n版本：{Info.Version ?? "未知"}\n种子：{(Info.Seed?.ToString() ?? "未知")}\n游戏模式：{GetGameModeText(Info.GameMode)}\n允许作弊：{(Info.AllowCommands is null ? "未知" : Info.AllowCommands.Value ? "是" : "否")}\n玩家数据：{Info.PlayerDataCount}\n数据包：{Info.DataPackArchiveCount}";

    private static string GetGameModeText(int? gameMode) =>
        gameMode switch { 0 => "生存", 1 => "创造", 2 => "冒险", 3 => "旁观", _ => "未知模式" };
}

public sealed class SaveImageLoader : IAsyncImageLoader
{
    public Task<Bitmap?> ProvideImageAsync(string url) => Task.Run<Bitmap?>(() =>
    {
        try
        {
            using var stream = File.OpenRead(url);
            return Bitmap.DecodeToWidth(stream, 112);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    });

    public void Dispose()
    {
    }
}