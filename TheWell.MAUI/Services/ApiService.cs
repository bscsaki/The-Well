using System.Net.Http.Headers;
using System.Net.Http.Json;
using TheWell.Core.DTOs;

namespace TheWell.MAUI.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";

    public ApiService(HttpClient http) => _http = http;

    private async Task AttachTokenAsync()
    {
        var token = await SecureStorage.GetAsync(AccessTokenKey);
        if (token is not null)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (result is not null)
        {
            await SecureStorage.SetAsync(AccessTokenKey, result.AccessToken);
            await SecureStorage.SetAsync(RefreshTokenKey, result.RefreshToken);
        }
        return result;
    }

    public async Task<bool> ForceResetAsync(ForceResetRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/force-reset", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RequestOtpAsync(string email)
    {
        var response = await _http.PostAsJsonAsync("api/auth/otp/request", new OtpRequestDto(email));
        return response.IsSuccessStatusCode;
    }

    public async Task<OtpVerifyResponse?> VerifyOtpAsync(string email, string otp)
    {
        var response = await _http.PostAsJsonAsync("api/auth/otp/verify", new OtpVerifyRequest(email, otp));
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<OtpVerifyResponse>();
    }

    public async Task<IntakeResponse?> GetIntakeAsync()
    {
        await AttachTokenAsync();
        var response = await _http.GetAsync("api/intake");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IntakeResponse>();
    }

    public async Task<IntakeResponse?> SubmitIntakeAsync(SubmitIntakeRequest request)
    {
        await AttachTokenAsync();
        var response = await _http.PostAsJsonAsync("api/intake", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IntakeResponse>();
    }

    public async Task<GoalResponse?> GetGoalAsync()
    {
        await AttachTokenAsync();
        var response = await _http.GetAsync("api/goals");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<GoalResponse>();
    }

    public async Task<GoalResponse?> CreateGoalAsync(CreateGoalRequest request)
    {
        await AttachTokenAsync();
        var response = await _http.PostAsJsonAsync("api/goals", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<GoalResponse>();
    }

    public async Task<List<DailyLogResponse>> GetLogsAsync()
    {
        await AttachTokenAsync();
        var result = await _http.GetFromJsonAsync<List<DailyLogResponse>>("api/logs");
        return result ?? [];
    }

    public async Task<DailyLogResponse?> CreateLogAsync(CreateLogRequest request)
    {
        await AttachTokenAsync();
        var response = await _http.PostAsJsonAsync("api/logs", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DailyLogResponse>();
    }

    public async Task<DailyLogResponse?> UpdateLogAsync(Guid logId, UpdateLogRequest request)
    {
        await AttachTokenAsync();
        var response = await _http.PutAsJsonAsync($"api/logs/{logId}", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DailyLogResponse>();
    }

    public async Task<StatsResponse?> GetStatsAsync()
    {
        await AttachTokenAsync();
        var response = await _http.GetAsync("api/stats");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<StatsResponse>();
    }

    public async Task<WeeklyContentResponse?> GetCurrentWeekContentAsync()
    {
        await AttachTokenAsync();
        var response = await _http.GetAsync("api/content/current-week");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<WeeklyContentResponse>();
    }

    public async Task<List<WeekSummaryResponse>> GetAllWeeksAsync()
    {
        await AttachTokenAsync();
        var result = await _http.GetFromJsonAsync<List<WeekSummaryResponse>>("api/content/weeks");
        return result ?? [];
    }

    public async Task<WeeklyContentResponse?> GetWeekContentAsync(int weekNumber)
    {
        await AttachTokenAsync();
        var response = await _http.GetAsync($"api/content/weeks/{weekNumber}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<WeeklyContentResponse>();
    }

    public async Task<UserProfileResponse?> GetProfileAsync()
    {
        await AttachTokenAsync();
        var response = await _http.GetAsync("api/users/me");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UserProfileResponse>();
    }

    public async Task<(bool success, string message)> ChangePasswordAsync(ChangePasswordRequest request)
    {
        await AttachTokenAsync();
        var response = await _http.PutAsJsonAsync("api/users/me/password", request);
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var msg = body.TryGetProperty("message", out var m) ? m.GetString() ?? ""
                : body.TryGetProperty("error", out var e) ? e.GetString() ?? "" : "";
        return (response.IsSuccessStatusCode, msg);
    }

    public async Task<DateOnly?> GetCourseStartDateAsync()
    {
        var response = await _http.GetAsync("api/config");
        if (!response.IsSuccessStatusCode) return null;
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        if (body.TryGetProperty("startDate", out var sd) && sd.ValueKind != System.Text.Json.JsonValueKind.Null)
        {
            if (DateOnly.TryParse(sd.GetString(), out var date)) return date;
        }
        return null;
    }

    public async Task<string> PopulateWeeksAsync()
    {
        var response = await _http.PostAsync("api/content/populate", null);
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        return body.TryGetProperty("message", out var m) ? m.GetString() ?? ""
             : body.TryGetProperty("error",   out var e) ? e.GetString() ?? "" : "";
    }

    public void ClearTokens()
    {
        SecureStorage.Remove(AccessTokenKey);
        SecureStorage.Remove(RefreshTokenKey);
    }
}
