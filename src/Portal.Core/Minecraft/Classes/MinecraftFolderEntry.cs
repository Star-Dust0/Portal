using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using MinecraftLaunch.Components.Parser;
using Tio.Avalonia.Standard.Modules.Extensions;
using TioUi.Common.Extensions;

namespace Portal.Core.Minecraft.Classes;

public partial class MinecraftFolderEntry : ObservableObject, IEquatable<MinecraftFolderEntry>
{
    [ObservableProperty] public partial string FolderName { get; set; }
    [ObservableProperty] public partial string FolderPath { get; set; }

    private int _instanceCount;
    private bool _isDataLoaded;
    private readonly Lock _lockObj = new();

    public string FolderSize
    {
        get
        {
            EnsureDataLoaded();
            return field;
        }
        set => SetProperty(ref field, value);
    } = "0.0";

    public string SizeUnit
    {
        get
        {
            EnsureDataLoaded();
            return field;
        }
        set => SetProperty(ref field, value);
    } = "B";

    public int InstanceCount
    {
        get
        {
            EnsureDataLoaded();
            return _instanceCount;
        }
        set => SetProperty(ref _instanceCount, value);
    }

    public MinecraftFolderEntry()
    {
        PropertyChanged += (_, e) => 
        { 
            if (e.PropertyName is nameof(FolderPath) or nameof(FolderName))
            {
                Events.RaiseCoreSaveSettings(); 
            }
        };
    }

    private void EnsureDataLoaded()
    {
        lock (_lockObj)
        {
            if (_isDataLoaded) return;
        }

        lock (_lockObj)
        {
            if (_isDataLoaded) return;

            if (!string.IsNullOrEmpty(FolderPath) && Directory.Exists(FolderPath))
            {
                try
                {
                    MinecraftParser parser = new(FolderPath);
                    _instanceCount = parser.GetMinecrafts().Count;
                }
                catch
                {
                    _instanceCount = 0;
                }

                _ = CalculateSizeAsync();
            }

            _isDataLoaded = true;
        }
    }

    private async Task CalculateSizeAsync()
    {
        if (string.IsNullOrEmpty(FolderPath) || !Directory.Exists(FolderPath)) return;

        var totalBytes = await Task.Run(() =>
        {
            try
            {
                var di = new DirectoryInfo(FolderPath);
                return di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
            }
            catch
            {
                return 0L;
            }
        });

        var a = ((double)totalBytes).GetReadableRaw(1);
        
        FolderSize = a.Value.ToString("F1");
        SizeUnit = a.Unit;
    }

    public void OpenFolder(object parameter)
    {
        var topLevel = (parameter as Control)?.GetTopLevel();
        topLevel?.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(FolderPath));
    }

    public bool Equals(MinecraftFolderEntry? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(FolderPath, other.FolderPath, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as MinecraftFolderEntry);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(FolderPath);
    }

    public static bool operator ==(MinecraftFolderEntry? left, MinecraftFolderEntry? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(MinecraftFolderEntry? left, MinecraftFolderEntry? right)
    {
        return !(left == right);
    }
}