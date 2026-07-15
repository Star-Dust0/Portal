using Avalonia.Media;
using Portal.Module.AggregatedSearch;
using Portal.ViewModels;
using Tio.Avalonia.Standard.Tab.Entries;
using Tio.Avalonia.Standard.Tab.Interface;
using Tio.Avalonia.Standard.Modules.Extensions; // 确保保留你原本的命名空间

namespace Portal.Views.Pages;

[AggregatedSearchPage("新闻", "新闻", "News")]
public partial class NewsPage : DataUserControl, ITioTabPage
{
    public NewsPageViewModel NewsPageViewModel { get; }

    public NewsPage()
    {
        InitializeComponent();
        
        NewsPageViewModel = NewsPageViewModel.Instance;
        DataContext = NewsPageViewModel;
    }
    
    public PageInfo PageInfo { get; init; } = new()
    {
        Title = "新闻",
        Icon = StreamGeometry.Parse(
            "F1 M640,640z M0,0z M128,96C128,78 142,64 160,64L480,64C498,64 512,78 512,96L512,544C512,562 498,576 480,576L160,576C142,576 128,562 128,544L128,96z M192,160L192,192H448V160H192z M192,256V288H448V256H192z M192,352V384H352V352H192z")
    };

    public TabEntry HostTab { get; set; }
}