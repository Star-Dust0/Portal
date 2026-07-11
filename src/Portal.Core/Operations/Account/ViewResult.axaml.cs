using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Portal.Core.Minecraft.Account;
using TioUi.Common.Interfaces;

namespace Portal.Core.Operations.Account;

public partial class ViewResult : UserControl
{
    public ViewResult()
    {
        InitializeComponent();
    }
}

public partial class ViewResultViewModel : ObservableObject, IDialogContext
{
    [ObservableProperty]
    public partial ObservableCollection<MinecraftAccount> Accounts { get; set; } = [];

    public ICommand CompleteCommand { get; }

    public ViewResultViewModel(ObservableCollection<MinecraftAccount> accounts)
    {
        Accounts = accounts;
        CompleteCommand = new RelayCommand(Complete);
    }

    private void Complete()
    {
        RequestClose?.Invoke(this, Accounts);
    }

    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;
}