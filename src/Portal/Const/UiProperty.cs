using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using Portal.Classes.Entries;
using Portal.Module.AggregatedSearch;
using Portal.Views.Components;
using Tio.Avalonia.Standard.Modules.Extensions;
using Tio.Avalonia.Standard.Standard.Ui;
using TioUi.Common.Classes;
using TioUi.Controls;

namespace Portal.Const;

public partial class UiProperty : ObservableObject
{
    private static UiProperty? _instance;

    public UiProperty()
    {
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName is nameof(AggregatedSearchQuery) or nameof(AggregatedSelectedType))
            {
                AggregatedSearchResults.Clear();
                AggregatedSearchResults.AddRange(Searcher.Search(AggregatedSearchQuery,
                    AggregatedSelectedType.EnumFlag));
            }
        };
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

    [ObservableProperty] public partial string AggregatedSearchQuery { get; set; }
    [ObservableProperty] public partial bool ConfigLoaded { get; set; }
    public ObservableCollection<AggregatedSearchEntry> AggregatedSearchResults { get; set; } = [];

    [ObservableProperty] public partial AggregatedSearchType AggregatedSelectedType { get; set; } = AggregatedSearchTypes[0];

    public static List<AggregatedSearchType> AggregatedSearchTypes { get; set; } =
    [
        new() { DisplayText = "所有", EnumFlag = AggregatedSearchEntryType.All },
        new() { DisplayText = "下级搜索", EnumFlag = AggregatedSearchEntryType.NextLevelSearch },
        new() { DisplayText = "账户", EnumFlag = AggregatedSearchEntryType.Account },
    ];
}