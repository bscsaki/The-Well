using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheWell.Core.DTOs;
using TheWell.Core.Entities;
using TheWell.Core.Interfaces;
using TheWell.Data.Repositories;

namespace TheWell.API.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize]
public class LogController(DailyLogRepository logRepo, ILockRuleService lockRule) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var logs = await logRepo.GetByUserAsync(CurrentUserId);
        return Ok(logs.Select(MapToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLogRequest request)
    {
        if (!lockRule.IsLogEditable(request.LogDate))
            return BadRequest(new { error = "Log date is outside the 5-day edit window" });

        if (await logRepo.ExistsForDateAsync(CurrentUserId, request.LogDate))
            return Conflict(new { error = "A log already exists for this date" });

        var log = new DailyLog
        {
            UserID = CurrentUserId,
            LogDate = request.LogDate,
            IsCompleted = request.IsCompleted,
            Note = request.Note
        };
        await logRepo.AddAsync(log);
        return CreatedAtAction(nameof(GetAll), MapToResponse(log));
    }

    [HttpPut("{logId:guid}")]
    public async Task<IActionResult> Update(Guid logId, [FromBody] UpdateLogRequest request)
    {
        var log = await logRepo.FindAsync(logId);
        if (log is null || log.UserID != CurrentUserId) return NotFound();

        if (!lockRule.IsLogEditable(log.LogDate))
            return BadRequest(new { error = "Log is outside the 5-day edit window" });

        log.IsCompleted = request.IsCompleted;
        log.Note = request.Note;
        log.IsLocked = false;
        await logRepo.SaveAsync();
        return Ok(MapToResponse(log));
    }

    private DailyLogResponse MapToResponse(DailyLog log) => new(
        log.LogID,
        log.LogDate,
        log.IsCompleted,
        log.Note,
        log.CreatedAt,
        !lockRule.IsLogEditable(log.LogDate));
}
