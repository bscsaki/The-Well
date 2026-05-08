using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheWell.API.Services;
using TheWell.Data.Repositories;

namespace TheWell.API.Controllers;

[ApiController]
[Route("api/content")]
[Authorize]
public class ContentController(
    ContentService contentService,
    CourseConfigRepository courseConfigRepo,
    WeekLockRepository weekLockRepo) : ControllerBase
{
    [HttpGet("current-week")]
    public async Task<IActionResult> GetCurrentWeek()
    {
        var config = await courseConfigRepo.GetCurrentAsync();
        int weekNumber;
        if (config is null)
            weekNumber = 1;
        else
        {
            var daysSinceStart = (DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - config.CourseStartDate.DayNumber);
            weekNumber = Math.Clamp(daysSinceStart / 7 + 1, 1, 60);
        }

        var result = await contentService.GetWeeklyContentAsync(weekNumber);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Returns all weeks from WordPress with their locked/unlocked status.
    /// If no course start date is set, all weeks are forced locked.</summary>
    [HttpGet("weeks")]
    public async Task<IActionResult> GetAllWeeks()
    {
        var weeks = await contentService.GetAllWeeksAsync();
        var config = await courseConfigRepo.GetCurrentAsync();

        // No start date = nothing is unlocked regardless of DB state
        if (config is null)
            return Ok(weeks.Select(w => w with { IsLocked = true }).ToList());

        return Ok(weeks);
    }

    /// <summary>Returns the full content for a specific week (must be unlocked).</summary>
    [HttpGet("weeks/{weekNumber:int}")]
    public async Task<IActionResult> GetWeek(int weekNumber)
    {
        var lockState = await weekLockRepo.GetAsync(weekNumber);
        bool isLocked = lockState?.IsLocked ?? true;
        if (isLocked) return Forbid();

        var result = await contentService.GetWeeklyContentAsync(weekNumber);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Admin: toggle lock/unlock for a week.</summary>
    [HttpPut("weeks/{weekNumber:int}/lock")]
    [AllowAnonymous] // Admin portal has no JWT — use AllowAnonymous for now
    public async Task<IActionResult> SetWeekLock(int weekNumber, [FromBody] WeekLockRequest request)
    {
        await weekLockRepo.SetLockAsync(weekNumber, request.IsLocked);
        return Ok(new { weekNumber, isLocked = request.IsLocked });
    }

    /// <summary>
    /// Admin: auto-unlock weeks based on the current date and the course start date.
    /// Week N is unlocked if today >= courseStart + (N-1)*7 days.
    /// </summary>
    [HttpPost("populate")]
    [AllowAnonymous]
    public async Task<IActionResult> Populate()
    {
        var config = await courseConfigRepo.GetCurrentAsync();
        if (config is null)
            return BadRequest(new { error = "No course start date set. Set a start date first." });

        var weeks = await contentService.GetAllWeeksAsync();
        if (weeks.Count == 0)
            return BadRequest(new { error = "No weeks found in WordPress. Add weekly content posts first." });

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int unlocked = 0;

        foreach (var week in weeks)
        {
            var weekStart = config.CourseStartDate.AddDays((week.WeekNumber - 1) * 7);
            bool isLocked = today < weekStart;
            await weekLockRepo.SetLockAsync(week.WeekNumber, isLocked);
            if (!isLocked) unlocked++;
        }

        return Ok(new
        {
            message  = $"Synced {weeks.Count} week(s). {unlocked} unlocked, {weeks.Count - unlocked} still locked.",
            total    = weeks.Count,
            unlocked
        });
    }

    [HttpDelete("cache")]
    [AllowAnonymous]
    public async Task<IActionResult> ClearCache()
    {
        await contentService.ClearAllCacheAsync();
        return Ok(new { message = "Content cache cleared." });
    }
}

public record WeekLockRequest(bool IsLocked);
