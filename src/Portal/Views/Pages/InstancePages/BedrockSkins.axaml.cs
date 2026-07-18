using Avalonia.Controls;
using Portal.Core.Minecraft.Classes;

namespace Portal.Views.Pages.InstancePages;

public partial class BedrockSkins : UserControl
{
    public BedrockSkins()
    {
        InitializeComponent();
    }

    public BedrockSkins(MinecraftInstance instance) : this()
    {
        SkinPacksContent.Content = new SkinPacks(instance);
    }
}
