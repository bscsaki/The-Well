using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheWell.Core.DTOs;
using TheWell.Core.Interfaces;
using TheWell.Data.Repositories;

namespace TheWell.API.Controllers;

[ApiController]
[Authorize]
public class UserController(
    UserRepository userRepo,
    CourseConfigRepository configRepo,
    IEncryptionService encryption) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());

    [HttpGet("api/users/me")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await userRepo.FindByIdAsync(CurrentUserId);
        if (user is null) return NotFound();
        return Ok(new UserProfileResponse(
            encryption.Decrypt(user.UniversityEID),
            encryption.Decrypt(user.Email)));
    }

    [HttpPut("api/users/me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await userRepo.FindByIdAsync(CurrentUserId);
        if (user is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return BadRequest(new { error = "Current password is incorrect." });

        if (request.NewPassword.Length < 8)
            return BadRequest(new { error = "New password must be at least 8 characters." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.IsPasswordResetRequired = false;
        await userRepo.SaveAsync();
        return Ok(new { message = "Password updated successfully." });
    }

    [HttpGet("api/config")]
    [AllowAnonymous]
    public async Task<IActionResult> GetConfig()
    {
        var config = await configRepo.GetCurrentAsync();
        if (config is null) return Ok(new { startDate = (DateOnly?)null });
        return Ok(new { startDate = config.CourseStartDate });
    }
}
