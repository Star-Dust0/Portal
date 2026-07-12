namespace Portal.Core.Minecraft.Classes;

public record AuthServer(AccountType AuthType, string DisplayText)
{
    public AccountType AuthType { get; set; } = AuthType;
    public string DisplayText { get; set; } = DisplayText;
    public string ServerUrl { get; set; }
}