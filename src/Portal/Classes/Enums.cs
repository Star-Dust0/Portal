namespace Portal.Classes.Enums;

public enum DefaultPage
{
    NewTabPage,
    SettingPage
}

public enum FilePicker
{
    System,
    Managed,
    Input
}

public enum NoticeWay
{
    Toast,
    Notification
}

public enum PortalVisibleMode
{
    NoOperation,
    QuitAfterLaunch,
    HiddenAfterLaunch,
    HiddenAfterLaunchAndReopen,
    MinimizedAfterLaunch,
    MinimizedAfterLaunchAndRestore
}