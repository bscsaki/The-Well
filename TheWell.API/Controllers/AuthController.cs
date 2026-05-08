using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheWell.API.Services;
using TheWell.Core.DTOs;
using TheWell.Core.Entities;
using TheWell.Core.Interfaces;
using TheWell.Data.Repositories;

namespace TheWell.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserRepository userRepo,
    AuditRepository auditRepo,
    IntakeRepository intakeRepo,
    IEncryptionService encryption,
    IOtpService otpService,
    IEmailService emailService,
    TokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var eidHash = encryption.Hash(request.ENumber);
        var user = await userRepo.FindByEidAsync(eidHash);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            if (user is not null)
                await auditRepo.AddAsync(new AuthenticationAudit
                    { UserID = user.UserID, Action = AuditActions.FailedLogin });
            return Unauthorized(new { error = "Invalid credentials" });
        }

        if (user.AccountStatus == AccountStatuses.Suspended)
            return Forbid();

        await auditRepo.AddAsync(new AuthenticationAudit
            { UserID = user.UserID, Action = AuditActions.Login });

        var intake = await GetIntakeStatus(user.UserID);

        return Ok(new LoginResponse(
            tokenService.GenerateAccessToken(user),
            tokenService.GenerateRefreshToken(),
            user.IsPasswordResetRequired,
            intake,
            user.AccountStatus));
    }

    [HttpPost("force-reset")]
    public async Task<IActionResult> ForceReset([FromBody] ForceResetRequest request)
    {
        var eidHash = encryption.Hash(request.ENumber);
        var user = await userRepo.FindByEidAsync(eidHash);
        if (user is null) return NotFound();
        if (!user.IsPasswordResetRequired) return BadRequest(new { error = "Password reset not required" });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.IsPasswordResetRequired = false;
        user.AccountStatus = AccountStatuses.Active;   // activate only after password change
        await userRepo.SaveAsync();

        await auditRepo.AddAsync(new AuthenticationAudit
            { UserID = user.UserID, Action = AuditActions.PasswordReset });

        return Ok(new { message = "Password updated successfully" });
    }

    [HttpPost("otp/request")]
    public async Task<IActionResult> RequestOtp([FromBody] OtpRequestDto request)
    {
        var encryptedEmail = encryption.Encrypt(request.Email);
        var user = await userRepo.FindByEncryptedEmailAsync(encryptedEmail);
        if (user is null) return Ok(new { message = "If that email exists, a code was sent" });

        var otp = otpService.Generate();
        var hash = otpService.Hash(otp);
        var expiry = DateTime.UtcNow.AddMinutes(15);

        await auditRepo.AddAsync(new AuthenticationAudit
        {
            UserID = user.UserID,
            Action = AuditActions.OtpRequest,
            OtpHash = hash,
            OtpExpiresAt = expiry
        });

        await emailService.SendOtpAsync(request.Email, otp);
        return Ok(new { message = "If that email exists, a code was sent" });
    }

    [HttpPost("otp/verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyRequest request)
    {
        var encryptedEmail = encryption.Encrypt(request.Email);
        var user = await userRepo.FindByEncryptedEmailAsync(encryptedEmail);
        if (user is null) return BadRequest(new { error = "Invalid code" });

        var latestOtp = await auditRepo.GetLatestOtpAsync(user.UserID);
        if (latestOtp?.OtpHash is null || latestOtp.OtpExpiresAt is null)
            return BadRequest(new { error = "No active OTP found" });

        if (!otpService.Verify(request.Otp, latestOtp.OtpHash, latestOtp.OtpExpiresAt.Value))
            return BadRequest(new { error = "Invalid or expired code" });

        await auditRepo.AddAsync(new AuthenticationAudit
            { UserID = user.UserID, Action = AuditActions.OtpVerify });

        user.IsPasswordResetRequired = true;
        await userRepo.SaveAsync();

        return Ok(new OtpVerifyResponse(tokenService.GeneratePasswordResetToken(user.UserID)));
    }

    private async Task<bool> GetIntakeStatus(Guid userId)
    {
        var intake = await intakeRepo.GetByUserAsync(userId);
        return intake is not null;
    }
}
