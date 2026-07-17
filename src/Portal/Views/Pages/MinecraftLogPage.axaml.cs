using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Portal.Core.Minecraft;
using Portal.Core.Minecraft.Classes;
using Tio.Avalonia.Standard.Tab.Entries;
using Tio.Avalonia.Standard.Tab.Interface;

namespace Portal.Views.Pages;

public partial class MinecraftLogPage : UserControl, ITioTabPage
{
    private const int MaximumVisibleLogLines = 10_000;
    private readonly MinecraftLogSession? _logSession;
    private readonly List<MinecraftLogEntry> _entries = [];

    public ObservableCollection<MinecraftLogLine> VisibleEntries { get; } = [];

    public MinecraftLogPage(MinecraftLogSession logSession)
    {
        _logSession = logSession;
        InitializeComponent();
        DataContext = this;
        _entries.AddRange(logSession.GetEntries());
        RefreshVisibleEntries();
        logSession.LogReceived += OnLogReceived;
    }

    public MinecraftLogPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public PageInfo PageInfo { get; init; } = new()
    {
        Title = "Minecraft 日志",
        Icon = StreamGeometry.Parse("M128,64L512,64C547.3,64,576,92.7,576,128L576,512C576,547.3,547.3,576,512,576L128,576C92.7,576,64,547.3,64,512L64,128C64,92.7,92.7,64,128,64z M160,176L160,240L480,240L480,176L160,176z M160,304L160,368L416,368L416,304L160,304z M160,432L160,496L352,496L352,432L160,432z")
    };

    public TabEntry HostTab { get; set; } = null!;

    public void OnClose()
    {
        if (_logSession != null)
            _logSession.LogReceived -= OnLogReceived;
    }

    public Task<bool> RequestCloseAsync() => Task.FromResult(true);

    public static void Open(MinecraftLogSession logSession, TopLevel sender)
    {
        if (sender is not TioTabWindowBase window)
            return;

        var tab = new TabEntry(window, new MinecraftLogPage(logSession)) { Title = $"{logSession.Instance.InstanceName} 日志" };
        window.CreateTab(tab);
        window.SelectTab(tab);
    }

    private void OnLogReceived(MinecraftLogEntry entry)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _entries.Add(entry);
            if (_entries.Count > MaximumVisibleLogLines)
                _entries.RemoveAt(0);
            if (IsLogLevelEnabled(entry.Level))
            {
                VisibleEntries.Add(new MinecraftLogLine(entry));
                if (VisibleEntries.Count > MaximumVisibleLogLines)
                    VisibleEntries.RemoveAt(0);
            }
            ScrollToLatest();
        });
    }

    private void FilterChanged(object? sender, RoutedEventArgs e)
    {
        if (InformationFilter == null)
            return;

        RefreshVisibleEntries();
        ScrollToLatest();
    }

    private void RefreshVisibleEntries()
    {
        VisibleEntries.Clear();
        foreach (var entry in _entries.Where(entry => IsLogLevelEnabled(entry.Level)))
            VisibleEntries.Add(new MinecraftLogLine(entry));
    }

    private bool IsLogLevelEnabled(MinecraftLogLevel level) => level switch
    {
        MinecraftLogLevel.Information => InformationFilter.IsChecked == true,
        MinecraftLogLevel.Warning => WarningFilter.IsChecked == true,
        MinecraftLogLevel.Error => ErrorFilter.IsChecked == true,
        MinecraftLogLevel.Debug => DebugFilter.IsChecked == true,
        MinecraftLogLevel.Trace => TraceFilter.IsChecked == true,
        _ => OtherFilter.IsChecked == true
    };

    private void ScrollToLatest()
    {
        if (AutoScrollCheckBox?.IsChecked == true && VisibleEntries.Count > 0)
            LogList.ScrollIntoView(VisibleEntries[^1]);
    }
}

public sealed class MinecraftLogLine(MinecraftLogEntry entry)
{
    public string Text { get; } = entry.Text;
    public IBrush Foreground { get; } = entry.Level switch
    {
        MinecraftLogLevel.Error => Brushes.IndianRed,
        MinecraftLogLevel.Warning => Brushes.Goldenrod,
        MinecraftLogLevel.Debug => Brushes.MediumPurple,
        MinecraftLogLevel.Trace => Brushes.SlateGray,
        MinecraftLogLevel.Information => Brushes.DodgerBlue,
        _ => Brushes.Gray
    };
}
