using System.Text.Json.Serialization;

namespace TheWell.MAUI.Services;

public class DailyContentResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public RenderedField Title { get; set; } = new();

    [JsonPropertyName("content")]
    public RenderedField Content { get; set; } = new();

    [JsonPropertyName("acf")]
    public DailyContentAcf Acf { get; set; } = new();
}

public class RenderedField
{
    [JsonPropertyName("rendered")]
    public string Rendered { get; set; } = string.Empty;
}

public class DailyContentAcf
{
    [JsonPropertyName("day_number")]
    public int DayNumber { get; set; }

    [JsonPropertyName("habit_goal")]
    public string HabitGoal { get; set; } = string.Empty;

    [JsonPropertyName("reflection_prompt")]
    public string ReflectionPrompt { get; set; } = string.Empty;

    [JsonPropertyName("week_theme")]
    public string WeekTheme { get; set; } = string.Empty;
}
