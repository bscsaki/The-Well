namespace TheWell.Core.DTOs;

public record ProvisionUserRequest(string UniversityEID, string Email);

public record UserResponse(
    Guid UserID,
    string UniversityEID,
    string Email,
    string AccountStatus,
    bool IsPasswordResetRequired,
    DateTime CreatedAt);

public record CourseConfigRequest(DateOnly CourseStartDate);

public record CourseConfigResponse(DateOnly CourseStartDate, DateOnly CourseEndDate, DateTime SetAt);

public record UserProfileResponse(string ENumber, string UniversityEmail);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
