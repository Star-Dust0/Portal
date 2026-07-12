using CommunityToolkit.Mvvm.ComponentModel;
using Portal.Classes.Entries;
using Tio.Avalonia.Standard.Modules.Platform;

namespace Portal.Const;

public partial class Data : ObservableObject
{
    private static Data? _instance;

    public static Data Instance
    {
        get { return _instance ??= new Data(); }
    }

    public static ConfigEntry ConfigEntry { get; set; }

    public static DesktopType DesktopType => DesktopTypeDetector.CurrentPlatform; 
    public static UiProperty UiProperty { get; } = UiProperty.Instance;
    [ObservableProperty] public partial string Version { get; set; }
}