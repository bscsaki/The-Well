using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheWell.Core.DTOs;
using TheWell.Core.Entities;
using TheWell.Core.Interfaces;
using TheWell.Data.Repositories;

namespace TheWell.API.Controllers;

[ApiController]
[Route("api/goals")]
[Authorize]
public class GoalController(GoalRepository goalRepo, ILockRuleService lockRule) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var goal = await goalRepo.GetByUserAsync(CurrentUserId);
        if (goal is null) return NotFound();
        return Ok(MapToResponse(goal));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoalRequest request)
    {
        var existing = await goalRepo.GetByUserAsync(CurrentUserId);
        if (existing is not null) return Conflict(new { error = "Goal already exists" });

        var goal = new Goal
        {
            UserID = CurrentUserId,
            GoalDefinition = request.GoalDefinition,
            LockedAt = DateTime.UtcNow.AddDays(5)
        };
        await goalRepo.AddAsync(goal);
        return CreatedAtAction(nameof(Get), MapToResponse(goal));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateGoalRequest request)
    {
        var goal = await goalRepo.GetByUserAsync(CurrentUserId);
        if (goal is null) return NotFound();
        if (lockRule.IsGoalLocked(goal.CreatedAt))
            return StatusCode(403, new { error = "Goal is locked (5-day window has passed)" });

        goal.GoalDefinition = request.GoalDefinition;
        await goalRepo.SaveAsync();
        return Ok(MapToResponse(goal));
    }

    private GoalResponse MapToResponse(Goal goal) => new(
        goal.GoalID,
        goal.GoalDefinition,
        goal.CreatedAt,
        goal.LockedAt,
        lockRule.IsGoalLocked(goal.CreatedAt));
}
