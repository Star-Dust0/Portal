using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using ReactiveUI;
using Tio.Avalonia.Standard.Standard.Ui;
using TioUi.Common.Classes;
using TioUi.Controls;

namespace Portal.Const;

public class UiProperty : ReactiveObject
{
    private static UiProperty? _instance;

    static UiProperty()
    {
    }

    public static UiProperty Instance
    {
        get { return _instance ??= new UiProperty(); }
    }

    public static ObservableCollection<NotificationEntry> Notifications { get; } = [];
    public static ObservableCollection<NotificationEntry> HistoryNotifications { get; } = [];
    public static TioToastManager Toast => ActiveWindow.Toast;
    public static ITioWindow ActiveWindow => (Application.Current!.ApplicationLifetime as
        IClassicDesktopStyleApplicationLifetime).Windows.FirstOrDefault
        (x => x.IsActive) as ITioWindow ?? App.MainWindow;
}