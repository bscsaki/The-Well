using System.Text.Json;
using TheWell.Core.DTOs;
using TheWell.Core.Entities;
using TheWell.Data.Repositories;

namespace TheWell.API.Services;

public class ContentService(
    MetadataCacheRepository cacheRepo,
    WeekLockRepository weekLockRepo,
    HttpClient httpClient,
    IConfiguration configuration)
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    public async Task ClearAllCacheAsync() => await cacheRepo.ClearAllAsync();

    /// <summary>Returns all weeks available in WordPress merged with their lock status.</summary>
    public async Task<List<WeekSummaryResponse>> GetAllWeeksAsync()
    {
        try
        {
            var wpBase = configuration["WordPress:BaseUrl"];
            var url = $"{wpBase}/wp-json/wp/v2/weekly_content?acf_format=standard&per_page=100&orderby=date&order=asc";
            var response = await httpClient.GetStringAsync(url);

            var items = JsonSerializer.Deserialize<JsonElement[]>(response);
            if (items is null || items.Length == 0) return [];

            var locks = await weekLockRepo.GetAllAsync();
            var lockMap = locks.ToDictionary(l => l.WeekNumber, l => l.IsLocked);

            var summaries = new List<WeekSummaryResponse>();
            foreach (var item in items)
            {
                if (!item.TryGetProperty("acf", out var acf)) continue;
                if (!acf.TryGetProperty("week_number", out var wn)) continue;

                int weekNum = wn.ValueKind == JsonValueKind.Number
                    ? wn.GetInt32()
                    : int.TryParse(wn.GetString(), out var p) ? p : 0;
                if (weekNum == 0) continue;

                int moduleNum = acf.TryGetProperty("module_number", out var mn)
                    ? (mn.ValueKind == JsonValueKind.Number ? mn.GetInt32() : weekNum) : weekNum;
                string title = acf.TryGetProperty("module_title", out var mt) ? mt.GetString() ?? "" : "";

                // Default: locked (true) unless admin has explicitly unlocked
                bool isLocked = lockMap.TryGetValue(weekNum, out var locked) ? locked : true;

                summaries.Add(new WeekSummaryResponse(weekNum, moduleNum, title, isLocked));
            }

            return summaries.OrderBy(s => s.WeekNumber).ToList();
        }
        catch
        {
            return [];
        }
    }

    public async Task<WeeklyContentResponse?> GetWeeklyContentAsync(int weekNumber)
    {
        // Try cache first — but wrap in try/catch in case stored payload is stale/wrong format
        try
        {
            var cacheKey = ContentTypes.WeeklyContent(weekNumber);
            var cached = await cacheRepo.GetValidAsync(cacheKey);
            if (cached is not null)
            {
                var cachedDto = JsonSerializer.Deserialize<WeeklyContentResponse>(cached.Payload);
                if (cachedDto is not null) return cachedDto;
            }
        }
        catch { /* stale cache — fall through to fetch fresh */ }

        try
        {
            var wpBase = configuration["WordPress:BaseUrl"];
            var url = $"{wpBase}/wp-json/wp/v2/weekly_content?acf_format=standard&per_page=100&orderby=date&order=desc";
            var response = await httpClient.GetStringAsync(url);

            var items = JsonSerializer.Deserialize<JsonElement[]>(response);
            if (items is null || items.Length == 0) return null;

            // Find exact week match first
            JsonElement? match = null;
            JsonElement? bestFallback = null;
            int bestFallbackWeek = 0;

            foreach (var item in items)
            {
                if (!item.TryGetProperty("acf", out var acf)) continue;
                if (!acf.TryGetProperty("week_number", out var wn)) continue;

                // Parse week number — handle both integer and string from ACF
                int postWeek = wn.ValueKind == JsonValueKind.Number
                    ? wn.GetInt32()
                    : int.TryParse(wn.GetString(), out var parsed) ? parsed : 0;

                if (postWeek == weekNumber) { match = item; break; }

                // Track best fallback: highest week number that is <= requested week
                if (postWeek < weekNumber && postWeek > bestFallbackWeek)
                {
                    bestFallbackWeek = postWeek;
                    bestFallback = item;
                }
            }

            // Use exact match, or fall back to nearest lower week
            var chosen = match ?? bestFallback ?? (items.Length > 0 ? items[0] : (JsonElement?)null);
            if (chosen is null) return null;

            var acfData = chosen.Value.TryGetProperty("acf", out var chosenAcf) ? chosenAcf : default;

            int resolvedWeek = weekNumber;
            if (acfData.TryGetProperty("week_number", out var rw))
                resolvedWeek = rw.ValueKind == JsonValueKind.Number ? rw.GetInt32()
                    : int.TryParse(rw.GetString(), out var p) ? p : weekNumber;

            var dto = new WeeklyContentResponse(
                WeekNumber: resolvedWeek,
                ModuleNumber: acfData.TryGetProperty("module_number", out var mn)
                    ? (mn.ValueKind == JsonValueKind.Number ? mn.GetInt32() : resolvedWeek) : resolvedWeek,
                ModuleTitle: acfData.TryGetProperty("module_title", out var mt) ? mt.GetString() ?? "" : "",
                MotivationalMessage: acfData.TryGetProperty("motivational_message", out var mm) ? mm.GetString() ?? "" : "",
                CourseMaterial: acfData.TryGetProperty("course_material", out var cm) ? cm.GetString() ?? "" : "",
                NotificationDay: acfData.TryGetProperty("day_1-7", out var nd)
                    ? (nd.ValueKind == JsonValueKind.Number ? nd.GetInt32() : 0) : 0,
                NotificationMessage: acfData.TryGetProperty("notifications", out var nm) ? nm.GetString() ?? "" : "");

            await cacheRepo.UpsertAsync(ContentTypes.WeeklyContent(resolvedWeek), JsonSerializer.Serialize(dto), CacheTtl);
            return dto;
        }
        catch
        {
            return null;
        }
    }
}
