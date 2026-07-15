using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Portal.Classes.Entries;
using Portal.Module.AggregatedSearch;
using Portal.Services;
using Portal.ViewModels;
using Tio.Avalonia.Standard.Modules.Extensions;
using Tio.Avalonia.Standard.Tab.Entries;
using Tio.Avalonia.Standard.Tab.Interface;

namespace Portal.Views.Pages;

[AggregatedSearchPage("新闻", "新闻", "News")]
public partial class NewsPage : DataUserControl, ITioTabPage
{
    public NewsPageViewModel NewsPageViewModel;

    public NewsPage()
    {
        InitializeComponent();
        NewsPageViewModel = new NewsPageViewModel();
        DataContext = NewsPageViewModel;
        Loaded += async (_, _) => await NewsPageViewModel.InitializeAsync();
    }

    public PageInfo PageInfo { get; init; } = new()
    {
        Title = "新闻",
        Icon = StreamGeometry.Parse(
            "F1 M640,640z M0,0z M128,96C128,78 142,64 160,64L480,64C498,64 512,78 512,96L512,544C512,562 498,576 480,576L160,576C142,576 128,562 128,544L128,96z M192,160L192,192H448V160H192z M192,256V288H448V256H192z M192,352V384H352V352H192z")
    };

    public TabEntry HostTab { get; set; }
}

public partial class NewsPageViewModel : ObservableObject
{
    private List<NewsEntry> _javaNews = [];
    private List<NewsEntry> _bedrockNews = [];

    public ObservableCollection<NewsEntry> FilteredNews { get; } = [];

    public List<NewsFilterOption> FilterOptions { get; } =
    [
        new() { DisplayText = "全部", Type = NewsFilterType.All },
        new() { DisplayText = "Java 版", Type = NewsFilterType.Java },
        new() { DisplayText = "基岩版", Type = NewsFilterType.Bedrock }
    ];

    [ObservableProperty] public partial bool IsVisible { get; set; }
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial NewsFilterOption? SelectedFilter { get; set; }

    public NewsPageViewModel()
    {
        SelectedFilter = FilterOptions[0];
    }

    partial void OnSelectedFilterChanged(NewsFilterOption? value) => ApplyFilter();

    public async Task InitializeAsync()
    {
        IsLoading = true;

        var javaCache = NewsService.LoadJavaCache();
        var bedrockCache = NewsService.LoadBedrockCache();

        _javaNews = javaCache;
        _bedrockNews = bedrockCache;

        if (_javaNews.Count > 0 || _bedrockNews.Count > 0)
        {
            ApplyFilter();
            IsVisible = true;

            _ = RefreshAsync();
        }
        else
        {
            await RefreshAsync();
        }

        IsLoading = false;
    }

    private async Task<bool> RefreshAsync()
    {
        var java = await NewsService.FetchJavaAsync();
        var bedrock = await NewsService.FetchBedrockAsync();

        bool hasAny = (java != null && java.Count > 0) || (bedrock != null && bedrock.Count > 0)
                   || _javaNews.Count > 0 || _bedrockNews.Count > 0;

        if (!hasAny)
        {
            IsVisible = false;
            return false;
        }

        if (java != null) _javaNews = java;
        if (bedrock != null) _bedrockNews = bedrock;

        ApplyFilter();
        return true;
    }

    private void ApplyFilter()
    {
        FilteredNews.Clear();
        var filter = SelectedFilter?.Type ?? NewsFilterType.All;
        IEnumerable<NewsEntry> list = filter switch
        {
            NewsFilterType.Java => _javaNews,
            NewsFilterType.Bedrock => _bedrockNews,
            _ => _javaNews.Concat(_bedrockNews).OrderByDescending(x => x.Date)
        };

        FilteredNews.AddRange(list);
    }
}

public class NewsFilterOption
{
    public string DisplayText { get; set; } = string.Empty;
    public NewsFilterType Type { get; set; }
}

public enum NewsFilterType
{
    All,
    Java,
    Bedrock
}
