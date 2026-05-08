namespace TheWell.Core.DTOs;

public record WeeklyContentResponse(
    int WeekNumber,
    int ModuleNumber,
    string ModuleTitle,
    string MotivationalMessage,
    string CourseMaterial,
    int NotificationDay,
    string NotificationMessage);

public record WeekSummaryResponse(
    int WeekNumber,
    int ModuleNumber,
    string ModuleTitle,
    bool IsLocked);
