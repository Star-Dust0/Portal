using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Portal.Core.Helpers;
using Portal.Core.Minecraft.Account;
using TioUi.Common;
using TioUi.Common.Interfaces;
using TioUi.Controls;

namespace Portal.Core.Operations.Account;

public partial class Yggdrasil : UserControl
{
    public Yggdrasil()
    {
        InitializeComponent();
    }
}

public partial class YggdrasilAccountViewModel : ObservableObject, IDialogContext, INotifyDataErrorInfo
{
    private readonly ObservableCollection<Minecraft.Account.AuthServer> _authServers;

    [ObservableProperty] public partial string? ServerUrl { get; set; }

    [ObservableProperty] public partial string? Username { get; set; }

    [ObservableProperty] public partial string? Password { get; set; }

    public List<Core.Minecraft.Account.AuthServer> BuiltInServers { get; } = [];

    private Core.Minecraft.Account.AuthServer? _selectedBuiltInServer;

    public Core.Minecraft.Account.AuthServer? SelectedBuiltInServer
    {
        get => _selectedBuiltInServer;
        set
        {
            SetProperty(ref _selectedBuiltInServer, value);
            if (value != null && !string.IsNullOrEmpty(value.ServerUrl))
            {
                ServerUrl = value.ServerUrl;
            }
        }
    }

    public ICommand NextCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand AuthServerCommand { get; }

    private readonly Dictionary<string, List<string>> _errors = new();

    public YggdrasilAccountViewModel(ObservableCollection<Core.Minecraft.Account.AuthServer> authServers)
    {
        _authServers = authServers;
        BuiltInServers.Add(new Core.Minecraft.Account.AuthServer(AccountType.Yggdrasil, "自定义"));
        BuiltInServers.Add(new Core.Minecraft.Account.AuthServer(AccountType.Yggdrasil, "LittleSkin")
        {
            ServerUrl = "https://littleskin.cn/api/yggdrasil"
        });

        foreach (var server in authServers)
        {
            bool exists = server.AuthType == AccountType.Yggdrasil
                ? BuiltInServers.Any(item => item.AuthType == AccountType.Yggdrasil &&
                                             !string.IsNullOrEmpty(item.ServerUrl) &&
                                             !string.IsNullOrEmpty(server.ServerUrl) &&
                                             UrlHelper.AreUrlsEqual(item.ServerUrl, server.ServerUrl))
                : BuiltInServers.Contains(server);

            if (!exists)
            {
                BuiltInServers.Add(server);
            }
        }

        if (SelectedBuiltInServer == null)
        {
            SelectedBuiltInServer = BuiltInServers[0];
        }

        NextCommand = new RelayCommand(Next, CanNext);
        CancelCommand = new RelayCommand(Cancel);
        AuthServerCommand = new RelayCommand(AddServer);
    }

    partial void OnServerUrlChanged(string? value)
    {
        ValidateServerUrl(value);
        (NextCommand as RelayCommand)?.NotifyCanExecuteChanged();
        
        if (!string.IsNullOrWhiteSpace(value))
        {
            var matchedServer = BuiltInServers.FirstOrDefault(server =>
                !string.IsNullOrEmpty(server.ServerUrl) &&
                UrlHelper.AreUrlsEqual(server.ServerUrl, value));
            
            if (matchedServer != null)
            {
                SelectedBuiltInServer = matchedServer;
            }
            else
            {
                SelectedBuiltInServer = BuiltInServers[0];
            }
        }
    }

    partial void OnUsernameChanged(string? value)
    {
        ValidateUsername(value);
        (NextCommand as RelayCommand)?.NotifyCanExecuteChanged();
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
            new AuthServerViewModel(_authServers.ToArray()), hostId: null, options: options);

        if (result != null)
        {
            _authServers.Add(result);
            BuiltInServers.Add(result);
            SelectedBuiltInServer = result;
        }
    }

    partial void OnPasswordChanged(string? value)
    {
        ValidatePassword(value);
        (NextCommand as RelayCommand)?.NotifyCanExecuteChanged();
    }

    private void ValidateServerUrl(string? value)
    {
        var propertyName = nameof(ServerUrl);

        if (_errors.ContainsKey(propertyName))
        {
            _errors.Remove(propertyName);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            _errors[propertyName] = new List<string> { "API 地址不能为空" };
        }
        else if (!UrlHelper.IsValidUrl(value))
        {
            _errors[propertyName] = new List<string> { "API 地址格式不正确" };
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void ValidateUsername(string? value)
    {
        var propertyName = nameof(Username);

        if (_errors.ContainsKey(propertyName))
        {
            _errors.Remove(propertyName);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            _errors[propertyName] = new List<string> { "账户不能为空" };
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void ValidatePassword(string? value)
    {
        var propertyName = nameof(Password);

        if (_errors.ContainsKey(propertyName))
        {
            _errors.Remove(propertyName);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            _errors[propertyName] = new List<string> { "密码不能为空" };
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    

    private bool CanNext()
    {
        return !HasErrors &&
               !string.IsNullOrWhiteSpace(ServerUrl) &&
               !string.IsNullOrWhiteSpace(Username) &&
               !string.IsNullOrWhiteSpace(Password);
    }

    private void Next()
    {
        RequestClose?.Invoke(this, new YggdrasilAccountResult
        {
            ServerUrl = ServerUrl!,
            Username = Username!,
            Password = Password!
        });
    }

    private void Cancel()
    {
        RequestClose?.Invoke(this, null);
    }

    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;

    public bool HasErrors => _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
        {
            return Enumerable.Empty<string>();
        }

        return _errors[propertyName];
    }
}

public class YggdrasilAccountResult
{
    public string ServerUrl { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}