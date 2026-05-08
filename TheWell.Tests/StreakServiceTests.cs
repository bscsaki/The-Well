using Microsoft.EntityFrameworkCore;
using TheWell.API.Services;
using TheWell.Core.Entities;
using TheWell.Data;
using TheWell.Data.Repositories;

namespace TheWell.Tests;

public class StreakServiceTests
{
    private static WellDbContext CreateDb() => TestDbContextFactory.Create();

    [Fact]
    public async Task Calculate_NoLogs_ReturnsZero()
    {
        using var db = CreateDb();
        var sut = new StreakService(new DailyLogRepository(db));
        Assert.Equal(0, await sut.CalculateAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Calculate_ThreeConsecutiveCompletedDays_ReturnsThree()
    {
        using var db = CreateDb();
        var userId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        db.DailyLogs.AddRange(
            new DailyLog { UserID = userId, LogDate = today, IsCompleted = true },
            new DailyLog { UserID = userId, LogDate = today.AddDays(-1), IsCompleted = true },
            new DailyLog { UserID = userId, LogDate = today.AddDays(-2), IsCompleted = true });
        await db.SaveChangesAsync();

        var sut = new StreakService(new DailyLogRepository(db));
        Assert.Equal(3, await sut.CalculateAsync(userId));
    }

    [Fact]
    public async Task Calculate_GapInLogs_StreakEndsAtGap()
    {
        using var db = CreateDb();
        var userId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        db.DailyLogs.AddRange(
            new DailyLog { UserID = userId, LogDate = today, IsCompleted = true },
            new DailyLog { UserID = userId, LogDate = today.AddDays(-1), IsCompleted = true },
            // gap at -2
            new DailyLog { UserID = userId, LogDate = today.AddDays(-3), IsCompleted = true });
        await db.SaveChangesAsync();

        var sut = new StreakService(new DailyLogRepository(db));
        Assert.Equal(2, await sut.CalculateAsync(userId));
    }

    [Fact]
    public async Task Calculate_TodayNotCompletedButYesterdayIs_CountsFromYesterday()
    {
        using var db = CreateDb();
        var userId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        db.DailyLogs.AddRange(
            new DailyLog { UserID = userId, LogDate = today, IsCompleted = false },
            new DailyLog { UserID = userId, LogDate = today.AddDays(-1), IsCompleted = true },
            new DailyLog { UserID = userId, LogDate = today.AddDays(-2), IsCompleted = true });
        await db.SaveChangesAsync();

        var sut = new StreakService(new DailyLogRepository(db));
        Assert.Equal(2, await sut.CalculateAsync(userId));
    }
}
