using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Portal.Classes.Entries;
using Portal.Const;
using Portal.Core.Minecraft.Account;
using Tio.Avalonia.Standard.Tab.Extensions;
using TopLevel = Avalonia.Controls.TopLevel;


namespace Portal.Module.AggregatedSearch;

public class Handler
{
    public static void Handle(AggregatedSearchEntry entry, TopLevel sender)
    {
        if (entry.Type == AggregatedSearchEntryType.Account)
        {
            var minecraftAccount = entry.Data as MinecraftAccount;
            Data.ConfigEntry.UsingMinecraftMinecraftAccount = minecraftAccount;
            sender.TryGetToast().Show($"已切换到 {minecraftAccount.Name}",NotificationType.Success);
        }
    }
}