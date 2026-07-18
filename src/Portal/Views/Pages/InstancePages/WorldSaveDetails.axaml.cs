using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Portal.Core.Minecraft.Classes;
using Portal.Core.Minecraft.Services;
using TioUi.Common.Interfaces;
using Tio.Avalonia.Standard.Tab.Gateway;

namespace Portal.Views.Pages.InstancePages;

public partial class WorldSaveDetails : UserControl
{
    public WorldSaveDetails()
    {
        InitializeComponent();
    }

    private void NavMenu_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if ((sender as TioUi.Controls.NavMenu)?.SelectedItem is TioUi.Controls.NavMenuItem { Tag: WorldSaveDetailsPage page })
            ((WorldSaveDetailsViewModel)DataContext).SelectedPage = page;
    }

    private void Close_OnClick(object? sender, RoutedEventArgs e) => ((WorldSaveDetailsViewModel)DataContext).Close();
}

public partial class WorldSaveOverview : UserControl
{
    public WorldSaveOverview() => InitializeComponent();
}

public partial class WorldSaveGameRules : UserControl
{
    public WorldSaveGameRules() => InitializeComponent();
}

public partial class WorldSaveWeather : UserControl
{
    public WorldSaveWeather() => InitializeComponent();
}

public partial class WorldSaveClocks : UserControl
{
    public WorldSaveClocks() => InitializeComponent();
}

public enum WorldSaveDetailsPage { Overview, GameRules, Weather, Clocks }

public partial class WorldSaveDetailsViewModel : ObservableObject, IDialogContext
{
    private readonly WorldSaveInfo _info;
    private readonly WorldGameRuleService _gameRuleService = new();
    private readonly WorldEnvironmentService _environmentService = new();
    private readonly WorldSaveService _worldSaveService = new();
    private WorldGameRules? _rules;

    [ObservableProperty] private WorldSaveDetailsPage _selectedPage = WorldSaveDetailsPage.Overview;
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private bool _hasGameRules;
    [ObservableProperty] private bool _hasWeather;
    [ObservableProperty] private bool _hasClocks;
    [ObservableProperty] private bool _raining;
    [ObservableProperty] private bool _thundering;
    [ObservableProperty] private string _rainTime = "0";
    [ObservableProperty] private string _thunderTime = "0";
    [ObservableProperty] private string _clearWeatherTime = "0";

    public ObservableCollection<WorldBooleanSetting> BooleanRules { get; } = [];
    public ObservableCollection<WorldNumberSetting> IntegerRules { get; } = [];
    public ObservableCollection<WorldNumberSetting> ClockSettings { get; } = [];
    public string DisplayName => string.IsNullOrWhiteSpace(_info.LevelName) ? _info.FolderName : _info.LevelName;
    public string FolderName => _info.FolderName;
    public string CreationTime => _info.CreationTime.ToString("yyyy-MM-dd HH:mm");
    public string LastPlayedTime => _info.LastPlayedTime?.ToString("yyyy-MM-dd HH:mm") ?? "未知";
    public string Version => _info.Version ?? "未知";
    public string Seed => _info.Seed?.ToString() ?? "未知";
    public string GameMode => _info.GameMode switch { 0 => "生存", 1 => "创造", 2 => "冒险", 3 => "旁观", _ => "未知" };
    public string AllowCommands => _info.AllowCommands is null ? "未知" : _info.AllowCommands.Value ? "是" : "否";
    public string FileStatistics => $"{_info.PlayerDataCount} 个玩家数据，{_info.DataPackArchiveCount} 个数据包";
    public bool IsLocked => _info.IsLocked;
    public bool IsOverview => SelectedPage == WorldSaveDetailsPage.Overview;
    public bool IsGameRules => SelectedPage == WorldSaveDetailsPage.GameRules;
    public bool IsWeather => SelectedPage == WorldSaveDetailsPage.Weather;
    public bool IsClocks => SelectedPage == WorldSaveDetailsPage.Clocks;

    public WorldSaveDetailsViewModel(WorldSaveInfo info)
    {
        _info = info;
        _ = LoadAsync();
    }

    partial void OnSelectedPageChanged(WorldSaveDetailsPage value)
    {
        OnPropertyChanged(nameof(IsOverview));
        OnPropertyChanged(nameof(IsGameRules));
        OnPropertyChanged(nameof(IsWeather));
        OnPropertyChanged(nameof(IsClocks));
    }

    private async Task LoadAsync()
    {
        try
        {
            _rules = await _gameRuleService.LoadAsync(_info.FolderPath);
            if (_rules != null)
            {
                foreach (var (key, value) in _rules.BooleanRules.OrderBy(x => x.Key))
                    BooleanRules.Add(new WorldBooleanSetting(key, key["minecraft:".Length..], value));
                foreach (var (key, value) in _rules.IntegerRules.OrderBy(x => x.Key))
                    IntegerRules.Add(new WorldNumberSetting(key, key["minecraft:".Length..], value));
                HasGameRules = true;
            }

            var weather = await _environmentService.LoadWeatherAsync(_info.FolderPath);
            if (weather != null)
            {
                Raining = weather.Raining;
                Thundering = weather.Thundering;
                RainTime = weather.RainTime.ToString();
                ThunderTime = weather.ThunderTime.ToString();
                ClearWeatherTime = weather.ClearWeatherTime.ToString();
                HasWeather = true;
            }

            var clocks = await _environmentService.LoadClocksAsync(_info.FolderPath);
            if (clocks != null)
            {
                foreach (var (dimension, ticks) in clocks.TotalTicks.OrderBy(x => x.Key))
                    ClockSettings.Add(new WorldNumberSetting(dimension, dimension, ticks));
                HasClocks = true;
            }
        }
        catch (Exception ex)
        {
            ShowNotice($"读取世界设置失败：{ex.Message}", NotificationType.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveGameRules()
    {
        if (_rules == null || !await CanSaveAsync()) return;
        if (!TryGetNumbers(IntegerRules, out var integers)) return;
        if (integers.Any(x => x.Value > int.MaxValue))
        {
            ShowNotice("游戏规则数值不能超过 2147483647", NotificationType.Warning);
            return;
        }
        _rules = new WorldGameRules(BooleanRules.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal), integers.ToDictionary(x => x.Key, x => (int)x.Value, StringComparer.Ordinal));
        await SaveAsync(() => _gameRuleService.SaveAsync(_info.FolderPath, _rules));
    }

    [RelayCommand]
    private async Task SaveWeather()
    {
        if (!HasWeather || !await CanSaveAsync()) return;
        if (!TryParseNonNegative(RainTime, out var rainTime) || !TryParseNonNegative(ThunderTime, out var thunderTime) || !TryParseNonNegative(ClearWeatherTime, out var clearWeatherTime))
        {
            ShowNotice("数值设置必须是非负整数", NotificationType.Warning);
            return;
        }
        if (rainTime > int.MaxValue || thunderTime > int.MaxValue || clearWeatherTime > int.MaxValue)
        {
            ShowNotice("天气时间不能超过 2147483647", NotificationType.Warning);
            return;
        }
        await SaveAsync(() => _environmentService.SaveWeatherAsync(_info.FolderPath,
            new WorldWeatherSettings(Raining, Thundering, (int)rainTime, (int)thunderTime, (int)clearWeatherTime)));
    }

    [RelayCommand]
    private async Task SaveClocks()
    {
        if (!HasClocks || !await CanSaveAsync()) return;
        if (!TryGetNumbers(ClockSettings, out var clocks)) return;
        await SaveAsync(() => _environmentService.SaveClocksAsync(_info.FolderPath, new WorldClockSettings(clocks.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal))));
    }

    private async Task<bool> CanSaveAsync()
    {
        if (await _worldSaveService.IsWorldLockedAsync(_info.FolderPath))
        {
            ShowNotice("世界正在被 Minecraft 使用，不能保存更改", NotificationType.Warning);
            return false;
        }
        return true;
    }

    private bool TryGetNumbers(IEnumerable<WorldNumberSetting> settings, out IReadOnlyList<(string Key, long Value)> values)
    {
        var result = new List<(string Key, long Value)>();
        foreach (var setting in settings)
        {
            if (!TryParseNonNegative(setting.Value, out var value))
            {
                ShowNotice("数值设置必须是非负整数", NotificationType.Warning);
                values = [];
                return false;
            }
            result.Add((setting.Key, value));
        }
        values = result;
        return true;
    }

    private static bool TryParseNonNegative(string? text, out long value) => long.TryParse(text, out value) && value >= 0;
    private async Task SaveAsync(Func<Task> save)
    {
        try { await save(); ShowNotice("设置已保存", NotificationType.Success); }
        catch (IOException ex) { ShowNotice($"保存失败：{ex.Message}", NotificationType.Error); }
        catch (UnauthorizedAccessException) { ShowNotice("没有修改此世界设置的权限", NotificationType.Error); }
    }
    private void ShowNotice(string message, NotificationType type)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
            NotificationGateway.Notice(window, message, type);
    }
    public void Close() => RequestClose?.Invoke(this, null);
    public event EventHandler<object?>? RequestClose;
}

public partial class WorldBooleanSetting(string key, string label, bool value) : ObservableObject
{
    public string Key { get; } = key;
    public string Label { get; } = label;
    [ObservableProperty] private bool _value = value;
}

public partial class WorldNumberSetting(string key, string label, long value) : ObservableObject
{
    public string Key { get; } = key;
    public string Label { get; } = label;
    [ObservableProperty] private string _value = value.ToString();
}
