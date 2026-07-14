using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Portal.ViewModels;

namespace Portal.Views.Pages.SettingPages;

public partial class Agent : DataUserControl
{
    public Agent()
    {
        InitializeComponent();
        DataContext = this;
    }
}