using System.Diagnostics;
using System.IO;
using Portal.Const;
using Portal.Views;
using Tio.Avalonia.Standard.Modules.Events;
using Tio.Avalonia.Standard.Modules.Platform;
using Tio.Avalonia.Standard.Tab.Common;
using TioUi.Common.Helpers;

namespace Portal.Module.Initialize;

public static class Initializer
{
    public static void App()
    {
        Config.Initialize();
    }

    public static void Ui()
    {
        File.WriteAllText(ConfigPath.AppPathDataPath,
            Process.GetCurrentProcess().MainModule.FileName);
        
        ThemeHelper.SetThemeColor(Data.ConfigEntry.ThemeColor);
        ThemeHelper.ToggleTheme(Data.ConfigEntry.Theme);
        
        LoopGc.BeginLoop();
        
        Functions.CreateNewTabWindowFunc = () => new TabWindow(false);
        
        InitializationEvents.RaiseAfterUiLoaded();
    }
}