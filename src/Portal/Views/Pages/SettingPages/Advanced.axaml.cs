using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Portal.ViewModels;

namespace Portal.Views.Pages.SettingPages;

public partial class Advanced : DataUserControl
{
    public Advanced()
    {
        InitializeComponent();
        DataContext = this;
    }
}