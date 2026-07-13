using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Portal.Classes.Entries;
using Portal.Const;
using Portal.Core.Minecraft.Classes;
using Portal.Core.Operations.Account;
using Tio.Avalonia.Standard.Tab.Extensions;
using Tio.Avalonia.Standard.Tab.Gateway;
using TioUi.Common;
using TioUi.Common.Extensions;
using TioUi.Controls;
using AuthServer = Portal.Core.Minecraft.Classes.AuthServer;
using TopLevel = Avalonia.Controls.TopLevel;


namespace Portal.Module.AggregatedSearch;

public class Handler
{
    public static void HandleAsync(AggregatedSearchEntry entry, TopLevel sender)
    {
        if (entry.Type == AggregatedSearchEntryType.Account)
        {
            var minecraftAccount = entry.Data as MinecraftAccount;
            Data.ConfigEntry.UsingMinecraftMinecraftAccount = minecraftAccount;
            NotificationGateway.Notice(sender, $"已切换到 {minecraftAccount.Name}", NotificationType.Success);
        }
        else if (entry.Type == AggregatedSearchEntryType.AuthServer)
        {
            _ = EditAuthServer(entry, sender);
        }
    }

    private static async Task EditAuthServer(AggregatedSearchEntry entry, TopLevel sender)
    {
        var authServer = entry.Data as AuthServer;
        if (authServer == null) return;

        var hostId = sender.TryGetHostId();

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

        var result = await OverlayDialog
            .ShowCustomAsync<EditAuthServer, EditAuthServerViewModel, EditAuthServerResult>(
                new EditAuthServerViewModel(authServer, Data.ConfigEntry.AuthServers.ToArray()),
                hostId: hostId, options: options);

        if (result != null)
        {
            if (result.IsDeleted)
            {
                Data.ConfigEntry.AuthServers.Remove(result.Server);
                NotificationGateway.Notice(sender, $"已删除验证服务器：{result.Server.DisplayText}", NotificationType.Success);
            }
            else
            {
                App.Method.SaveConfig();
                NotificationGateway.Notice(sender, "验证服务器已更新", NotificationType.Success);
            }
        }
    }
}