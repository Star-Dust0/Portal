using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Portal.Classes.Entries;
using Portal.Const;
using Tio.Avalonia.Standard.Modules.DiskIO;

namespace Portal.Services;

public static class NewsService
{
    private static readonly HttpClient Client = new();

    private const string JavaApiUrl = "https://launchercontent.mojang.com/v2/javaPatchNotes.json";
    private const string BedrockApiUrl = "https://launchercontent.mojang.com/v2/bedrockPatchNotes.json";
    private const string BaseImageUrl = "https://launchercontent.mojang.com";

    private static string JavaCachePath => Path.Combine(ConfigPath.UserDataRootPath, "java_news_cache.json");
    private static string BedrockCachePath => Path.Combine(ConfigPath.UserDataRootPath, "bedrock_news_cache.json");

    public static List<NewsEntry> LoadJavaCache() => LoadCache(JavaCachePath, NewsEdition.Java);
    public static List<NewsEntry> LoadBedrockCache() => LoadCache(BedrockCachePath, NewsEdition.Bedrock);

    public static async Task<List<NewsEntry>?> FetchJavaAsync() => await FetchAsync(JavaApiUrl, JavaCachePath, NewsEdition.Java);
    public static async Task<List<NewsEntry>?> FetchBedrockAsync() => await FetchAsync(BedrockApiUrl, BedrockCachePath, NewsEdition.Bedrock);

    private static List<NewsEntry> LoadCache(string path, NewsEdition edition)
    {
        try
        {
            if (!File.Exists(path)) return [];
            var json = File.ReadAllText(path);
            var response = JsonConvert.DeserializeObject<PatchNotesResponse>(json);
            return response?.Entries?.Select(e => MapToNewsEntry(e, edition)).ToList() ?? [];
        }
        catch (Exception ex)
        {
            Logger.Error($"加载新闻缓存失败: {ex.Message}");
            return [];
        }
    }

    private static async Task<List<NewsEntry>?> FetchAsync(string url, string cachePath, NewsEdition edition)
    {
        try
        {
            var json = await Client.GetStringAsync(url);
            File.WriteAllText(cachePath, json);
            var response = JsonConvert.DeserializeObject<PatchNotesResponse>(json);
            return response?.Entries?.Select(e => MapToNewsEntry(e, edition)).ToList() ?? [];
        }
        catch (Exception ex)
        {
            Logger.Error($"获取新闻失败 ({url}): {ex.Message}");
            return null;
        }
    }

    private static NewsEntry MapToNewsEntry(PatchNoteEntry entry, NewsEdition edition)
    {
        var imageUrl = string.Empty;
        if (!string.IsNullOrEmpty(entry.Image?.Url))
        {
            imageUrl = entry.Image.Url.StartsWith("http") ? entry.Image.Url : BaseImageUrl + entry.Image.Url;
        }

        return new NewsEntry
        {
            Title = entry.Title,
            Version = entry.Version,
            Type = !string.IsNullOrEmpty(entry.Type) ? entry.Type : entry.PatchNoteType,
            ImageUrl = imageUrl,
            ContentPath = entry.ContentPath,
            Id = entry.Id,
            Date = entry.Date.ToLocalTime(),
            ShortText = entry.ShortText,
            NeedsTranslation = entry.NeedsTranslation,
            Edition = edition
        };
    }
}
