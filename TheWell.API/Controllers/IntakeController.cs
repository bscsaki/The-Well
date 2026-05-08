using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheWell.Core.DTOs;
using TheWell.Core.Entities;
using TheWell.Data.Repositories;

namespace TheWell.API.Controllers;

[ApiController]
[Route("api/intake")]
[Authorize]
public class IntakeController(IntakeRepository intakeRepo) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var intake = await intakeRepo.GetByUserAsync(CurrentUserId);
        if (intake is null) return NotFound();
        return Ok(MapResponse(intake));
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitIntakeRequest request)
    {
        var existing = await intakeRepo.GetByUserAsync(CurrentUserId);
        if (existing?.IsUnlocked == true)
            return Conflict(new { error = "Intake already completed" });

        var intake = existing ?? new IntakeQuestion { UserID = CurrentUserId };
        intake.MyHabit = request.MyHabit;
        intake.MyGoal = request.MyGoal;
        intake.IAmPersonWho = request.IAmPersonWho;
        intake.Strategy1 = request.Strategy1;
        intake.Strategy2 = request.Strategy2;
        intake.ToImproveMyselfIWill = request.ToImproveMyselfIWill;
        intake.RewardMyselfWith = request.RewardMyselfWith;
        intake.PeopleForEncouragement = request.PeopleForEncouragement;
        intake.CompletedAt = DateTime.UtcNow;
        intake.IsUnlocked = true;

        if (existing is null)
            await intakeRepo.AddAsync(intake);
        else
            await intakeRepo.SaveAsync();

        return Ok(MapResponse(intake));
    }

    private static IntakeResponse MapResponse(IntakeQuestion i) => new(
        i.MyHabit, i.MyGoal, i.IAmPersonWho,
        i.Strategy1, i.Strategy2,
        i.ToImproveMyselfIWill, i.RewardMyselfWith,
        i.PeopleForEncouragement, i.IsUnlocked, i.CompletedAt);
}
