using System.Diagnostics;
using System.IO;
using Avalonia;
using Portal.Const;
using Portal.Core.Minecraft;
using Portal.Views;
using Tio.Avalonia.Standard.Modules.Events;
using Tio.Avalonia.Standard.Modules.Extensions;
using Tio.Avalonia.Standard.Modules.Platform;
using Tio.Avalonia.Standard.Tab.Common;
using TioUi.Common.Helpers;

namespace Portal.Module.Initialize;

public static class Initializer
{
    public static void App()
    {
        Config.Initialize();
        MinecraftCoreInitializer.Initialize(Data.Instance.Version);
    }

    public static void Ui()
    {
        File.WriteAllText(ConfigPath.AppPathDataPath,
            Process.GetCurrentProcess().MainModule.FileName);
        
        ThemeHelper.SetThemeColor(Data.ConfigEntry.ThemeColor);
        ThemeHelper.ToggleTheme(Data.ConfigEntry.Theme);
        
        LoopGc.BeginLoop();
        
        Functions.CreateNewTabWindowFunc = _ => new TabWindow(false);
        
                
        Data.UiProperty.AggregatedSearchResults.Clear();
        Data.UiProperty.AggregatedSearchResults.AddRange(
            AggregatedSearch.Searcher.Search(
                Data.UiProperty.AggregatedSearchQuery, 
                Data.UiProperty.AggregatedSelectedType.EnumFlag));

        
        InitializationEvents.RaiseAfterUiLoaded();
    }
}