using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI.Fody.Helpers;
using Tio.Avalonia.Standard.Modules.Platform;

namespace Portal.Const;

public class Data : ObservableObject
{
    private static Data? _instance;

    public static Data Instance
    {
        get { return _instance ??= new Data(); }
    }

    public static DesktopType DesktopType => DesktopTypeDetector.CurrentPlatform; 
    public static UiProperty UiProperty { get; } = UiProperty.Instance;
    [Reactive] public string Version { get; set; }
}