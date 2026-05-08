namespace TheWell.Core.DTOs;

public record LoginRequest(string ENumber, string Password);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    bool IsPasswordResetRequired,
    bool IsIntakeComplete,
    string AccountStatus);

public record ForceResetRequest(string ENumber, string NewPassword);

public record OtpRequestDto(string Email);

public record OtpVerifyRequest(string Email, string Otp);

public record OtpVerifyResponse(string PasswordResetToken);

public record RefreshRequest(string RefreshToken);
