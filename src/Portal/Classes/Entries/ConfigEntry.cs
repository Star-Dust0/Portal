using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Portal.Classes.Enums;
using Portal.Core.Minecraft.Account;
using Tio.Avalonia.Standard.Tab.Gateway;
using TioUi.Common.Helpers;
using TioUi.Shared;

namespace Portal.Classes.Entries;

public partial class ConfigEntry : ObservableObject
{
    public ConfigEntry()
    {
        PropertyChanged += OnPropertyChanged;
        MinecraftAccounts.CollectionChanged += (_, _) => App.Method.SaveConfig();
        AuthServers.CollectionChanged += (_, _) => App.Method.SaveConfig();
    }

    [ObservableProperty] public partial Theme Theme { get; set; } = Theme.Light;
    [ObservableProperty] public partial DefaultPage DefaultPage { get; set; } = DefaultPage.NewTabPage;
    [ObservableProperty] public partial Color ThemeColor { get; set; } = Color.Parse("#1890ff");
    [ObservableProperty] public partial NoticeWay NoticeWay { get; set; } = NoticeWay.Toast;
    [ObservableProperty] public partial FilePicker FilePicker { get; set; } = FilePicker.System;
    public ObservableCollection<MinecraftAccount> MinecraftAccounts { get; } = [];
    public ObservableCollection<AuthServer> AuthServers { get; } = [];
    [ObservableProperty] public partial MinecraftAccount? UsingMinecraftMinecraftAccount { get; set; }
    [ObservableProperty] public partial bool ShowDragDropPrompt { get; set; } = true;

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Theme):
                ThemeHelper.ToggleTheme(Theme);
                break;
            case nameof(ThemeColor):
                ThemeHelper.SetThemeColor(ThemeColor);
                break;
        }

        App.Method.SaveConfig();
    }
}