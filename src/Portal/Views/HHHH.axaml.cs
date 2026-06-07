using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Tio.Avalonia.Standard.Tab.Entries;
using Tio.Avalonia.Standard.Tab.Interface;

namespace Portal.Views;

public partial class HHHH : UserControl,ITioTabPage
{
    public HHHH()
    {
        InitializeComponent();
    }

    public PageInfo PageInfo { get; init; } = new()
    {
        Title = "归给我干活",
        Icon = StreamGeometry.Parse(
            "F1 M1024,1024z M0,0z M512,832A32,32,0,0,0,544,800L544,544 800,544A32,32,0,0,0,800,480L544,480 544,224A32,32,0,0,0,480,224L480,480 224,480A32,32,0,0,0,224,544L480,544 480,800A32,32,0,0,0,512,832"),
    };

    public TabEntry HostTab { get; set; }
}