using Portal.Core.Minecraft.Instance;
using Portal.Core.Minecraft.Instance.Bedrock;

var manager = new InstanceManager(@"D:\Games\.minecraft");
manager.RefreshInstances().ForEach(x =>
{
    Console.WriteLine($"{x.Type} {x.Version}");
    if (x.Type == InstanceType.Bedrock)
    {
        var bedrockConf = BedrockHelper.GetInstanceConfig(x.InstanceFolder);
        Console.WriteLine($"Bedrock Config: Name={bedrockConf.Name}, Version={bedrockConf.Version}, BuildType={bedrockConf.BuildType}, Type={bedrockConf.Type}");
    }
});