using System.Net.Http.Json;

namespace TheWell.MAUI.Services;

public class WordPressContentService
{
    private readonly HttpClient _http;
    // Base URL is set at registration time via MauiProgram.cs / HttpClient configuration
    private const string Endpoint = "/wp-json/wp/v2/daily_content";

    public WordPressContentService(HttpClient http) => _http = http;

    public async Task<List<DailyContentResponse>> GetAllDaysAsync()
    {
        var result = await _http.GetFromJsonAsync<List<DailyContentResponse>>(
            $"{Endpoint}?per_page=60&orderby=date&order=asc");
        return result ?? [];
    }

    public async Task<DailyContentResponse?> GetDayAsync(int dayNumber)
    {
        var all = await GetAllDaysAsync();
        return all.FirstOrDefault(d => d.Acf.DayNumber == dayNumber);
    }
}
