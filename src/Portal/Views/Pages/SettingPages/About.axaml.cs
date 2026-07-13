using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using Portal.Const;
using Portal.Module.Update;
using Portal.ViewModels;
using Tio.Avalonia.Standard.Tab.Extensions;
using Tio.Avalonia.Standard.Tab.Gateway;

namespace Portal.Views.Pages.SettingPages;

public partial class About : DataUserControl
{
    public readonly AboutViewModel AboutViewModel;

    public About()
    {
        InitializeComponent();
        AboutViewModel = new AboutViewModel();
        DataContext = AboutViewModel;
        if (Data.Version.Type == "dev")
            UpdateChannel.IsEnabled = false;
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Data.UiProperty.IsLatestVersion = false;
        Data.UiProperty.FoundNewVersion = false;
        if (Data.UiProperty.OverrideUpdateChannel == "dev")
        {
            sender!.AsTopLevel().Notice("开发版本(dev)不能更新", NotificationType.Error);
            return;
        }

        var channel = Data.UiProperty.OverrideUpdateChannel;
        if (channel != "nightly" && channel != "commit")
        {
            return;
        }

        HyperlinkButton.Content = "检查更新中";
        HyperlinkButton.IsEnabled = false;
        var result = await CheckUpdate.Main(sender!.AsTopLevel());
        HyperlinkButton.Content = "检查更新";
        HyperlinkButton.IsEnabled = true;
        if (result == "latest")
        {
            Data.UiProperty.IsLatestVersion = true;
            return;
        }
        Data.UiProperty.NewVersion = result;
        Data.UiProperty.FoundNewVersion = true;
    }
}

public partial class AboutViewModel : ObservableObject
{
    public Data Data => Data.Instance;
    public string Version { get; } =
        $"v{Data.Instance.Version.Version}-{Data.Instance.Version.Type}-{Data.Instance.Version.BuildTime:yyyy.MMdd.HHmm}-" +
        $"{Data.Instance.Version.Action}-{Data.Instance.Version.Commit}";
}