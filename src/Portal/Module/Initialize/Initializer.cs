using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls.Notifications;
using Portal.Classes.Entries;
using Portal.Classes.Enums;
using Portal.Const;
using Portal.Core;
using Portal.Core.Minecraft;
using Portal.Views;
using Tio.Avalonia.Standard.Modules.Events;
using Tio.Avalonia.Standard.Modules.Extensions;
using Tio.Avalonia.Standard.Modules.Platform;
using Tio.Avalonia.Standard.Tab.Common;
using Tio.Avalonia.Standard.Tab.Gateway;
using TioUi.Common.Helpers;

namespace Portal.Module.Initialize;

public static class Initializer
{
    public static void App()
    {
        Config.Initialize();
        MinecraftCoreInitializer.Initialize(Data.Instance.Version.VersionTitle);
    }

    public static void Ui()
    {
        File.WriteAllText(ConfigPath.AppPathDataPath,
            Process.GetCurrentProcess().MainModule.FileName);

        ThemeHelper.SetThemeColor(Data.ConfigEntry.ThemeColor);
        ThemeHelper.ToggleTheme(Data.ConfigEntry.Theme);
        if (Data.ConfigEntry.EnableCustomForegroundColor)
        {
            ConfigEntry.SetForegroundColor(Data.ConfigEntry.ForegroundColor);
        }

        LoopGc.BeginLoop();

        Functions.CreateNewTabWindowFunc = _ => new TabWindow(false);
        NotificationGateway.IsToastFunc = () => Data.ConfigEntry.NoticeWay == NoticeWay.Toast;

        Events.CoreSaveSettings += Portal.App.Method.SaveConfig;

        if (Data.ConfigEntry.BackgroundMode == BackgroundMode.Default)
            Application.Current.Resources.Remove("BackGroundOpacity");
        else
            Application.Current.Resources["BackGroundOpacity"] = Data.ConfigEntry.ControlOpacity;

        InitializationEvents.RaiseAfterUiLoaded();
    }
}