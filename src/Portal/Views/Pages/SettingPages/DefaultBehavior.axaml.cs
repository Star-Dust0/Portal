using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Portal.ViewModels;
using TioUi.Common.Extensions;

namespace Portal.Views.Pages.SettingPages;

public partial class DefaultBehavior : DataUserControl
{
    public DefaultBehavior()
    {
        InitializeComponent();
        DataContext = this;
    }
}