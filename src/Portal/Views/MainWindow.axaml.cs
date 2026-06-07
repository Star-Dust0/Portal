using Avalonia.Controls;
using Tio.Avalonia.Standard.Standard.Ui;
using TioUi.Controls;

namespace Portal.Views;

public partial class MainWindow : TioWindow, ITioWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Notification = new TioNotificationManager(this);
        Toast = new TioToastManager(this);
        Window = this;
    }

    public TioNotificationManager Notification { get; set; }
    public TioToastManager Toast { get; set; }
    public TioWindow Window { get; set; }
}