using System.Text.Json;
using TheWell.Core.DTOs;

namespace TheWell.MAUI.Services;

public static class ContentCacheService
{
    private const string WeeklyContentKey = "cache_weekly_content";
    private const string WeeklyContentExpiryKey = "cache_expiry_weekly_content";

    public static WeeklyContentResponse? GetCachedWeeklyContent()
    {
        if (!IsCacheValid(WeeklyContentExpiryKey)) return null;
        var json = Preferences.Get(WeeklyContentKey, null);
        return json is null ? null : JsonSerializer.Deserialize<WeeklyContentResponse>(json);
    }

    public static void SetWeeklyContent(WeeklyContentResponse dto)
    {
        Preferences.Set(WeeklyContentKey, JsonSerializer.Serialize(dto));
        Preferences.Set(WeeklyContentExpiryKey, DateTime.UtcNow.AddHours(24).ToString("O"));
    }

    private static bool IsCacheValid(string expiryKey)
    {
        var expiry = Preferences.Get(expiryKey, null);
        if (expiry is null) return false;
        return DateTime.TryParse(expiry, out var dt) && DateTime.UtcNow < dt;
    }
}
