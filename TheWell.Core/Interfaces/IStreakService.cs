namespace TheWell.Core.Interfaces;

public interface IStreakService
{
    Task<int> CalculateAsync(Guid userId);
}
