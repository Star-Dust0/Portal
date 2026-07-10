using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Portal.Core.Minecraft.Account;
using TioUi.Common;
using TioUi.Controls;
using TioUi.Common.Interfaces;

namespace Portal.Core.Operations.Account;

public partial class SelectAccountType : UserControl
{
    public SelectAccountType()
    {
        InitializeComponent();
    }
}

public partial class SelectAccountTypeViewModel : ObservableObject, IDialogContext
{
    public ObservableCollection<Minecraft.Account.AuthServer> AuthServers { get; } = [];

    private Minecraft.Account.AuthServer? _selectedServer;
    public Minecraft.Account.AuthServer? SelectedServer
    {
        get => _selectedServer;
        set
        {
            SetProperty(ref _selectedServer, value);
            (NextCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }
    }

    public ICommand NextCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand AddServerCommand { get; }

    private readonly ObservableCollection<Minecraft.Account.AuthServer> _originalAuthServers;

    public SelectAccountTypeViewModel(ObservableCollection<Minecraft.Account.AuthServer> authServers)
    {
        _originalAuthServers = authServers;

        AuthServers.Add(new Minecraft.Account.AuthServer(AccountType.Offline, "离线模式"));
        AuthServers.Add(new Minecraft.Account.AuthServer(AccountType.Microsoft, "微软账户"));
        AuthServers.Add(new Minecraft.Account.AuthServer(AccountType.Yggdrasil, "外置登录"));
        AuthServers.Add(new Minecraft.Account.AuthServer(AccountType.Yggdrasil, "LittleSkin")
        {
            ServerUrl = "https://littleskin.cn/api/yggdrasil"
        });

        foreach (var server in authServers)
        {
            bool exists = server.AuthType == AccountType.Yggdrasil
                ? AuthServers.Any(item => item.AuthType == AccountType.Yggdrasil &&
                                          !string.IsNullOrEmpty(item.ServerUrl) &&
                                          string.Equals(item.ServerUrl, server.ServerUrl, StringComparison.OrdinalIgnoreCase))
                : AuthServers.Contains(server);

            if (!exists)
            {
                AuthServers.Add(server);
            }
        }

        NextCommand = new RelayCommand(Next, CanNext);
        CancelCommand = new RelayCommand(Cancel);
        AddServerCommand = new RelayCommand(AddServer);

        SelectedServer = AuthServers.FirstOrDefault();
    }



    private bool CanNext()
    {
        return SelectedServer != null;
    }

    private void Next()
    {
        RequestClose?.Invoke(this, new SelectAccountTypeResult(SelectAccountTypeAction.Select, SelectedServer));
    }

    private void Cancel()
    {
        RequestClose?.Invoke(this, new SelectAccountTypeResult(SelectAccountTypeAction.Cancel));
    }

    private async void AddServer()
    {
        var options = new OverlayDialogOptions
        {
            Mode = DialogMode.None,
            Buttons = DialogButton.None,
            CanLightDismiss = false,
            CanDragMove = true,
            IsCloseButtonVisible = false,
            CanResize = false,
            VerticalOffset = 110,
            VerticalAnchor = VerticalPosition.Top
        };

        var result = await OverlayDialog.ShowCustomAsync<AuthServer, AuthServerViewModel, Minecraft.Account.AuthServer>(
            new AuthServerViewModel(AuthServers.ToArray()), hostId: null, options: options);

        if (result != null)
        {
            AuthServers.Add(result);
            _originalAuthServers.Add(result);
            SelectedServer = result;
        }
    }

    public void Close()
    {
        RequestClose?.Invoke(this, new SelectAccountTypeResult(SelectAccountTypeAction.Cancel));
    }

    public event EventHandler<object?>? RequestClose;
}

public enum SelectAccountTypeAction
{
    Cancel,
    Select
}

public class SelectAccountTypeResult
{
    public SelectAccountTypeAction Action { get; }
    public Minecraft.Account.AuthServer? SelectedServer { get; }

    public SelectAccountTypeResult(SelectAccountTypeAction action, Minecraft.Account.AuthServer? selectedServer = null)
    {
        Action = action;
        SelectedServer = selectedServer;
    }
}