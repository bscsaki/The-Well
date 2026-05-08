namespace TheWell.Core.DTOs;

public record DailyLogResponse(
    Guid LogID,
    DateOnly LogDate,
    bool IsCompleted,
    string? Note,
    DateTime CreatedAt,
    bool IsLocked);

public record CreateLogRequest(DateOnly LogDate, bool IsCompleted, string? Note);

public record UpdateLogRequest(bool IsCompleted, string? Note);
