namespace Portal.Core.Minecraft.Account;

public class MicrosoftAccount() : AccountBase(AccountType.Microsoft), IEquatable<MicrosoftAccount>
{
    public string Name { get; set; } = string.Empty;
    public Guid Uuid { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public DateTime LastRefreshTime { get; set; }

    public bool Equals(MicrosoftAccount? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Uuid.Equals(other.Uuid);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as MicrosoftAccount);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MinecraftLaunch.Base.Enums.AccountType.Microsoft, Uuid);
    }

    public static bool operator ==(MicrosoftAccount? left, MicrosoftAccount? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(MicrosoftAccount? left, MicrosoftAccount? right)
    {
        return !(left == right);
    }
}