using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Portal.Const;
using Portal.ViewModels;
using Tio.Avalonia.Standard.Modules.Extensions;
using TioUi.Common.Extensions;

namespace Portal.Views.Pages.SettingPages;

public partial class Storage : UserControl
{
    public StorageViewModel ViewModel { get; }

    public Storage()
    {
        InitializeComponent();
        ViewModel = new StorageViewModel();
        DataContext = ViewModel;
    }

    public void TriggerRefresh()
    {
        _ = ViewModel.RefreshStorageDataAsync();
    }

    private void OpenFolder_Click(object? sender, RoutedEventArgs e)
    {
        _ = (sender as Control)!.GetTopLevel().Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(ConfigPath.UserDataRootPath));
    }
}

public partial class StorageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsWindowsPlatform { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalSizeString))]
    [NotifyPropertyChangedFor(nameof(PortalSizeString))]
    [NotifyPropertyChangedFor(nameof(PortalPercentage))]
    public partial double PortalBytesRaw { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalSizeString))]
    [NotifyPropertyChangedFor(nameof(GameSizeString))]
    public partial double GameBytesRaw { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PortalPercentage))]
    [NotifyPropertyChangedFor(nameof(SystemDriveSizeString))]
    public partial double SystemDriveBytesRaw { get; set; }

    public string TotalSizeString => (PortalBytesRaw + GameBytesRaw).ToHumanReadableSize(1);
    public string PortalSizeString => PortalBytesRaw.ToHumanReadableSize(1);
    public string GameSizeString => GameBytesRaw.ToHumanReadableSize(1);
    public string SystemDriveSizeString => SystemDriveBytesRaw.ToHumanReadableSize(0);

    public double PortalPercentage
    {
        get
        {
            if (SystemDriveBytesRaw <= 0) return 0;
            var percent = (PortalBytesRaw / SystemDriveBytesRaw) * 100;
            return Math.Min(100, Math.Max(0, percent));
        }
    }

    private readonly string _portalDataPath = ConfigPath.UserDataRootPath;
    private readonly string _gameDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Portal", "Games");

    public StorageViewModel()
    {
        IsWindowsPlatform = false;
        _ = RefreshStorageDataAsync();
    }

    [RelayCommand]
    public async Task RefreshStorageDataAsync()
    {
        string portalPath = _portalDataPath;
        string gamePath = _gameDataPath;
        bool isWin = IsWindowsPlatform;

        await Task.Run(() =>
        {
            try
            {
                double driveTotalBytes = 0;

                if (isWin)
                {
                    var driveInfo = new DriveInfo("C:\\");
                    driveTotalBytes = driveInfo.TotalSize;
                }

                (long portalBytes, long gameBytes) = GetPortalAndGameSizes(portalPath, gamePath);

                PortalBytesRaw = portalBytes;
                GameBytesRaw = gameBytes;
                SystemDriveBytesRaw = driveTotalBytes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        });
    }

    private (long PortalBytes, long GameBytes) GetPortalAndGameSizes(string portalPath, string gamePath)
    {
        if (!Directory.Exists(portalPath)) return (0, 0);

        long totalPortalBytes = 0;
        long totalGameBytes = 0;

        try
        {
            var di = new DirectoryInfo(portalPath);
            
            Parallel.ForEach(di.EnumerateFiles("*", SearchOption.AllDirectories), 
                () => (PortalSum: 0L, GameSum: 0L),
                (fileInfo, loopState, localState) =>
                {
                    try
                    {
                        long fileSize = fileInfo.Length;
                        
                        if (fileInfo.FullName.StartsWith(gamePath, StringComparison.OrdinalIgnoreCase))
                        {
                            localState.GameSum += fileSize;
                        }
                        else
                        {
                            localState.PortalSum += fileSize;
                        }
                    }
                    catch (FileNotFoundException) {} 
                    catch (UnauthorizedAccessException) {} 

                    return localState;
                },
                localResult =>
                {
                    Interlocked.Add(ref totalPortalBytes, localResult.PortalSum);
                    Interlocked.Add(ref totalGameBytes, localResult.GameSum);
                });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Directory walk failed: {ex.Message}");
        }

        return (totalPortalBytes, totalGameBytes);
    }
}