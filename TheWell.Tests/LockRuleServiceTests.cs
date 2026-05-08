using TheWell.API.Services;

namespace TheWell.Tests;

public class LockRuleServiceTests
{
    private readonly LockRuleService _sut = new();

    [Fact]
    public void IsLogEditable_TodaysLog_ReturnsTrue()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Assert.True(_sut.IsLogEditable(today));
    }

    [Fact]
    public void IsLogEditable_FiveDaysAgo_ReturnsTrue()
    {
        var fiveDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5);
        Assert.True(_sut.IsLogEditable(fiveDaysAgo));
    }

    [Fact]
    public void IsLogEditable_SixDaysAgo_ReturnsFalse()
    {
        var sixDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-6);
        Assert.False(_sut.IsLogEditable(sixDaysAgo));
    }

    [Fact]
    public void IsGoalLocked_CreatedJustNow_ReturnsFalse()
    {
        Assert.False(_sut.IsGoalLocked(DateTime.UtcNow));
    }

    [Fact]
    public void IsGoalLocked_CreatedFiveDaysAgo_ReturnsFalse()
    {
        Assert.False(_sut.IsGoalLocked(DateTime.UtcNow.AddDays(-4).AddHours(-23)));
    }

    [Fact]
    public void IsGoalLocked_CreatedSixDaysAgo_ReturnsTrue()
    {
        Assert.True(_sut.IsGoalLocked(DateTime.UtcNow.AddDays(-6)));
    }
}
