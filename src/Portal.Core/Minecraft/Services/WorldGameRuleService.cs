using fNbt;
using Portal.Core.Minecraft.Classes;

namespace Portal.Core.Minecraft.Services;

public sealed class WorldGameRuleService
{
    private const string GameRulesRelativePath = "data/minecraft/game_rules.dat";

    public Task<WorldGameRules?> LoadAsync(string worldPath, CancellationToken cancellationToken = default) =>
        Task.Run(() => Load(worldPath, cancellationToken), cancellationToken);

    public Task SaveAsync(string worldPath, WorldGameRules rules, CancellationToken cancellationToken = default) =>
        Task.Run(() => Save(worldPath, rules, cancellationToken), cancellationToken);

    private static WorldGameRules? Load(string worldPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = Path.Combine(worldPath, GameRulesRelativePath);
        if (!File.Exists(path))
            return null;

        var file = new NbtFile();
        file.LoadFromFile(path);
        var data = file.RootTag["data"] as NbtCompound;
        if (data == null)
            return null;

        var booleanRules = new Dictionary<string, bool>(StringComparer.Ordinal);
        var integerRules = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var tag in data.Tags)
        {
            if (string.IsNullOrEmpty(tag.Name))
                continue;

            var name = tag.Name!;

            switch (tag)
            {
                case NbtByte value:
                    booleanRules[name] = value.Value != 0;
                    break;
                case NbtInt value:
                    integerRules[name] = value.Value;
                    break;
            }
        }

        return new WorldGameRules(booleanRules, integerRules);
    }

    private static void Save(string worldPath, WorldGameRules rules, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = Path.Combine(worldPath, GameRulesRelativePath);
        var file = new NbtFile();
        file.LoadFromFile(path);
        var data = file.RootTag["data"] as NbtCompound
                   ?? throw new InvalidDataException("游戏规则文件不包含 data 标签。");

        foreach (var (name, value) in rules.BooleanRules)
        {
            if (data[name] is NbtByte tag)
                tag.Value = value ? (byte)1 : (byte)0;
        }
        foreach (var (name, value) in rules.IntegerRules)
        {
            if (data[name] is NbtInt tag)
                tag.Value = value;
        }

        // Java Edition's per-world data files are uncompressed NBT.
        file.SaveToFile(path, NbtCompression.None);
    }
}
