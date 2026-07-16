using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MinecraftLaunch.Base.Models.Game;
using Portal.Core.Minecraft.Classes;
using Portal.Core.Minecraft.Instance.Java;
using Portal.ViewModels;
using Tio.Avalonia.Standard.Modules.DiskIO;
using Tio.Avalonia.Standard.Modules.Extensions;
using TioUi.Common.Extensions;

namespace Portal.Views.Pages.InstancePages;

public partial class Dashboard : DataUserControl
{
    public MinecraftInstance Instance { get; }

    public string TotalPlayTime
    {
        get
        {
            var seconds = Instance.GetTotalPlayTimeSeconds();
            return seconds < 60 ? $"{seconds} 秒" :
                seconds < 3600 ? $"{seconds / 60.0:F1} 分钟" : $"{seconds / 3600.0:F1} 小时";
        }
    }

    public Dashboard(MinecraftInstance instance)
    {
        Instance = instance;
        InitializeComponent();
        DataContext = this;
    }

    public Dashboard()
    {
        InitializeComponent();
    }

    private void OpenFolder_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Control control)
            _ = (sender as Control)!.GetTopLevel().Launcher
                .LaunchDirectoryInfoAsync(new DirectoryInfo(Instance.InstanceFolderPath));
    }
}