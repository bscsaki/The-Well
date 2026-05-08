namespace TheWell.Core.Entities;

public class MetadataCache
{
    public Guid CacheID { get; set; } = Guid.NewGuid();
    public string ContentType { get; set; } = string.Empty;  // 'WeeklyTopic' | 'DailyMotivation'
    public string Payload { get; set; } = string.Empty;       // JSONB stored as string
    public DateTime ExpiryDate { get; set; }
}

public static class ContentTypes
{
    public static string WeeklyContent(int weekNumber) => $"WeeklyContent_W{weekNumber}";
}
