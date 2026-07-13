using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using Portal.Const;
using Portal.ViewModels;

namespace Portal.Views.Pages.SettingPages;

public partial class About : DataUserControl
{
    public About()
    {
        InitializeComponent();
        DataContext = new AboutViewModel();
    }
}

public partial class AboutViewModel : ObservableObject
{
    [ObservableProperty] public partial bool IsLatest { get; set; }
    public Data Data => Data.Instance;

    public string Version { get; } =
        $"v{Data.Instance.Version.Version}-{Data.Instance.Version.Type}-{Data.Instance.Version.BuildTime:yyyy.MMdd.HHmm}-" +
        $"{Data.Instance.Version.Action}-{Data.Instance.Version.Commit}";
}