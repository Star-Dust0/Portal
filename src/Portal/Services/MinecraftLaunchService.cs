using System.IO;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using MinecraftLaunch.Base.Models.Authentication;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Components.Parser;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Launch;
using Portal.Const;
using Portal.Core.Minecraft.Classes;
using Portal.Core.Minecraft.Instance.Java;
using Portal.Core.Operations.Account;
using Tio.Avalonia.Standard.Modules.Tasks;
using Tio.Avalonia.Standard.Tab.Gateway;
using TioUi.Common;
using Avalonia.Controls.Notifications;

namespace Portal.Services;

public static class MinecraftLaunchService
{
    public static Task LaunchAsync(MinecraftInstance instance, TopLevel? topLevel)
    {
        var task = TaskManager.Instance.CreateTask(new TaskOptions
        {
            Name = $"启动 {instance.InstanceName}",
            Description = "正在准备启动流程",
            Progress = 0
        });
        var verifyAccount = task.CreateChild(new TaskOptions
        {
            Name = "验证游戏账户", Description = "等待验证", Progress = 0
        });
        var selectJava = task.CreateChild(new TaskOptions
        {
            Name = "选择 Java 运行时", Description = "等待账户验证完成", Progress = 0
        });
        var buildArguments = task.CreateChild(new TaskOptions
        {
            Name = "构建启动参数", Description = "等待 Java 运行时选择完成", Progress = 0
        });
        var startGame = task.CreateChild(new TaskOptions
        {
            Name = "启动 Minecraft", Description = "等待启动参数构建完成", Progress = 0
        });
        task.Start();
        _ = RunWorkflowAsync(instance, topLevel, task, verifyAccount, selectJava, buildArguments, startGame);
        return task.Completion;
    }

    private static async Task RunWorkflowAsync(MinecraftInstance instance, TopLevel? topLevel, ManagedTask task,
        ManagedTask verifyAccount, ManagedTask selectJava, ManagedTask buildArguments, ManagedTask startGame)
    {
        try
        {
            if (instance.Type != MinecraftInstanceType.Java || instance.MinecraftEntry == null)
                throw new InvalidOperationException("当前仅支持启动 Java 版 Minecraft 实例。");

            Account? account = null;
            JavaEntry? java = null;
            LaunchConfig? config = null;

            verifyAccount.Start(async context =>
            {
                context.SetRunning("正在验证游戏账户");
                account = await VerifyAccountAsync();
                context.ReportProgress(1);
            });
            await verifyAccount.Completion;
            ThrowIfFailed(verifyAccount);

            selectJava.Start(async context =>
            {
                context.SetRunning("正在检查可用 Java 运行时");
                java = await SelectJavaAsync(instance, context.CancellationToken);
                context.ReportProgress(1);
            });
            await selectJava.Completion;
            ThrowIfFailed(selectJava);

            buildArguments.Start(context =>
            {
                context.SetRunning("正在应用实例与全局游戏设置");
                config = CreateLaunchConfig(instance, account!, java!);
                context.ReportProgress(1);
                return Task.CompletedTask;
            });
            await buildArguments.Completion;
            ThrowIfFailed(buildArguments);

            startGame.Start(context => StartGameStepAsync(context, instance, config!, topLevel, task));
            await startGame.Completion;
            ThrowIfFailed(startGame);
        }
        catch (Exception exception)
        {
            if (!task.IsTerminal)
                task.Fault(exception);
            Notice(topLevel, $"启动失败：{GetFailureReason(exception)}", NotificationType.Error);
        }
    }

    private static void ThrowIfFailed(ManagedTask task)
    {
        if (task.Exception != null)
            throw new InvalidOperationException(task.Exception.Message, task.Exception);
        task.CancellationToken.ThrowIfCancellationRequested();
    }

    private static async Task StartGameStepAsync(TaskExecutionContext context, MinecraftInstance instance,
        LaunchConfig config, TopLevel? topLevel, ManagedTask task)
    {
        context.SetRunning("正在启动 Minecraft 进程");
        var parser = new MinecraftParser(instance.MinecraftEntry!.MinecraftFolderPath);
        var process = await new MinecraftRunner(config, parser)
            .RunAsync(instance.MinecraftEntry, context.CancellationToken);
        if (process == null)
            throw new InvalidOperationException("Minecraft 启动器未返回进程信息。");
        ObserveProcess(instance, topLevel, process, task);
        context.ReportProgress(1);
    }

    private static async Task<Account> VerifyAccountAsync()
    {
        var account = Data.ConfigEntry.UsingMinecraftMinecraftAccount
                      ?? throw new InvalidOperationException("请先在账户设置中选择用于启动游戏的账户。");
        if (string.IsNullOrWhiteSpace(account.Name))
            throw new InvalidOperationException("所选账户没有有效的玩家名。");

        switch (account.AccountType)
        {
            case AccountType.Offline:
                return new OfflineAccount(account.Name, account.Uuid ?? MinecraftAccount.GetMinecraftOfflineUuid(account.Name),
                    account.AccessToken ?? Guid.NewGuid().ToString("N"));
            case AccountType.Yggdrasil:
                if (!account.Uuid.HasValue || string.IsNullOrWhiteSpace(account.AccessToken) ||
                    string.IsNullOrWhiteSpace(account.ClientToken) || string.IsNullOrWhiteSpace(account.YggdrasilServerUrl))
                    throw new InvalidOperationException("外置登录账户信息不完整，请重新登录。");
                return new YggdrasilAccount(account.Name, account.Uuid.Value, account.AccessToken, account.ClientToken,
                    account.YggdrasilServerUrl) { MetaData = account.MetaData };
            case AccountType.Microsoft:
                var refreshed = await AccountRefresher.RefreshMicrosoft(account)
                                ?? throw new InvalidOperationException("微软账户令牌刷新失败，请重新登录。");
                UpdateMicrosoftAccount(account, refreshed);
                if (!refreshed.Uuid.HasValue || string.IsNullOrWhiteSpace(refreshed.AccessToken) ||
                    string.IsNullOrWhiteSpace(refreshed.RefreshToken))
                    throw new InvalidOperationException("微软账户刷新后缺少必要的验证信息。");
                return new MicrosoftAccount(refreshed.Name, refreshed.Uuid.Value, refreshed.AccessToken,
                    refreshed.RefreshToken, refreshed.LastRefreshTime ?? DateTime.Now);
            default:
                throw new InvalidOperationException("不支持的账户类型。");
        }
    }

    private static void UpdateMicrosoftAccount(MinecraftAccount original, MinecraftAccount refreshed)
    {
        var accounts = Data.ConfigEntry.MinecraftAccounts;
        var index = accounts.IndexOf(original);
        if (index >= 0)
            accounts[index] = refreshed;
        Data.ConfigEntry.UsingMinecraftMinecraftAccount = refreshed;
    }

    private static async Task<JavaEntry> SelectJavaAsync(MinecraftInstance instance, CancellationToken cancellationToken)
    {
        var preferred = instance.Config.EnableSpecificJava ? instance.Config.SpecificJavaEntry : null;
        var candidates = preferred != null ? [preferred] : Data.ConfigEntry.JavaRuntimes.ToList();
        if (candidates.Count == 0 && Data.ConfigEntry.EnableAutoSelectJava)
            candidates = (await JavaRuntimeManager.ScanAsync(cancellationToken)).ToList();
        if (candidates.Count == 0)
            throw new InvalidOperationException("没有可用的 Java 运行时，请在设置中添加 Java。");

        var javaEntries = candidates.Select(ToJavaEntry).ToList();
        var selected = preferred != null ? javaEntries[0] : Data.ConfigEntry.EnableAutoSelectJava
            ? instance.MinecraftEntry!.GetAppropriateJava(javaEntries)
            : Data.ConfigEntry.DefaultJavaRuntime is { } defaultJava ? ToJavaEntry(defaultJava) : javaEntries[0];
        return selected ?? throw new InvalidOperationException("找不到与当前 Minecraft 版本兼容的 Java 运行时。");
    }

    private static JavaEntry ToJavaEntry(JavaRuntimeEntry java) => new()
    {
        JavaPath = java.JavaPath, JavaType = java.JavaType, JavaVersion = java.JavaVersion,
        MajorVersion = java.MajorVersion, Is64bit = java.Is64Bit
    };

    private static LaunchConfig CreateLaunchConfig(MinecraftInstance instance, Account account, JavaEntry java) => new()
    {
        Account = account,
        JavaPath = java,
        LauncherName = "Portal",
        IsEnableIndependency = instance.Config.EnableIndependentInstance,
        Width = Data.ConfigEntry.MinecraftWindowWidth,
        Height = Data.ConfigEntry.MinecraftWindowHeight,
        MinMemorySize = 512,
        MaxMemorySize = instance.Config.EnableOverrideMaxMemory
            ? instance.Config.MinecraftMaxMemory
            : Data.ConfigEntry.MinecraftMaxMemory
    };

    private static void ObserveProcess(MinecraftInstance instance, TopLevel? topLevel, MinecraftProcess process,
        ManagedTask task)
    {
        instance.Config.LastPlayTime = DateTime.Now;
        instance.IncrementPlaySessions();
        instance.StartPlayTimer();
        task.AddAction(new TaskActionDefinition
        {
            Name = "结束进程",
            Description = "结束 Minecraft 及其子进程。",
            ExecuteAsync = (_, _) =>
            {
                var nativeProcess = process.Process;
                if (nativeProcess == null)
                    throw new InvalidOperationException("Minecraft 进程尚未创建或已无法访问。");
                if (!nativeProcess.HasExited)
                    nativeProcess.Kill(entireProcessTree: true);
                return Task.CompletedTask;
            },
            IsVisible = _ => IsProcessRunning(process)
        });
        task.AddAction(new TaskActionDefinition
        {
            Name = "复制启动参数",
            Description = "复制本次启动使用的完整 Java 参数。",
            ExecuteAsync = async (_, _) =>
            {
                if (topLevel?.Clipboard == null)
                    throw new InvalidOperationException("当前窗口不支持访问系统剪贴板。");
                await topLevel.Clipboard.SetTextAsync(string.Join(Environment.NewLine, process.ArgumentList));
            }
        });
        process.Exited += (_, _) =>
        {
            instance.StopPlayTimer();
            Notice(topLevel, $"{instance.InstanceName} 已退出", NotificationType.Success);
            Dispatcher.UIThread.Post(() =>
            {
                if (!task.IsTerminal)
                    task.Complete();
            });
        };

        if (!IsProcessRunning(process))
        {
            instance.StopPlayTimer();
            task.Complete();
        }
    }

    private static bool IsProcessRunning(MinecraftProcess process)
    {
        try
        {
            return process.Process is { HasExited: false };
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static void Notice(TopLevel? topLevel, string message, NotificationType type)
    {
        if (topLevel == null)
            return;
        Dispatcher.UIThread.Post(() => NotificationGateway.Notice(topLevel, message, type));
    }

    private static string GetFailureReason(Exception exception) => exception switch
    {
        FileNotFoundException => "缺少游戏或 Java 文件。",
        UnauthorizedAccessException => "没有访问游戏目录或 Java 文件的权限。",
        _ => exception.Message
    };
}
