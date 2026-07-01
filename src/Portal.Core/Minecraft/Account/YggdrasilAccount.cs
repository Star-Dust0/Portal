using Newtonsoft.Json;

namespace Portal.Core.Minecraft.Account;

public class YggdrasilAccount() : AccountBase(AccountType.Yggdrasil), IEquatable<YggdrasilAccount>
{
    public string Name { get; set; } = string.Empty;
    public string? ServerNote { get; set; }
    public Guid Uuid { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string YggdrasilServerUrl { get; set; } = string.Empty;
    public string ClientToken { get; set; } = string.Empty;
    public Dictionary<string, string> MetaData { get; set; } = [];

    [JsonIgnore]
    public string AccountNote =>
        ServerNote ?? (Uri.TryCreate(YggdrasilServerUrl, UriKind.Absolute, out var uri) ? uri.Host : "外置登录");

    public bool Equals(YggdrasilAccount? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Uuid.Equals(other.Uuid) && string.Equals(YggdrasilServerUrl, other.YggdrasilServerUrl,
            StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as YggdrasilAccount);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AccountType.Yggdrasil, Uuid,
            StringComparer.OrdinalIgnoreCase.GetHashCode(YggdrasilServerUrl));
    }

    public static bool operator ==(YggdrasilAccount? left, YggdrasilAccount? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(YggdrasilAccount? left, YggdrasilAccount? right)
    {
        return !(left == right);
    }
}