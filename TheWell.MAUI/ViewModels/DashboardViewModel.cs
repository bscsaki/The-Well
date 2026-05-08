using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TheWell.Core.DTOs;
using TheWell.MAUI.Services;
using TheWell.MAUI.Views;

namespace TheWell.MAUI.ViewModels;

public record FeedItem(string Type, string Title, string Body);

public class CalendarDay
{
    public DateOnly Date { get; init; }
    public int GoalDay { get; init; }           // 1–56 (course day number)
    public bool IsCompleted { get; init; }
    public bool IsToday { get; init; }
    public bool IsFuture { get; init; }
    public bool IsEditable { get; init; }
    public string? ModuleTitle { get; init; }   // non-null on Mondays only

    // Top half: actual calendar date  e.g. "15"
    public string DateLabel => Date.Day.ToString();
    // Bottom half: goal tracking day  e.g. "G·1"
    public string GoalLabel => $"G·{GoalDay}";

    public bool HasModuleTitle => !string.IsNullOrEmpty(ModuleTitle);

    // Full square background colour
    public Color BackgroundColor =>
        IsToday     ? Color.FromArgb("#FF9500") :
        IsCompleted ? Color.FromArgb("#1A6B8A") :
        IsFuture    ? Color.FromArgb("#E8E8E8") :
        IsEditable  ? Color.FromArgb("#FFCCCC") :
                      Color.FromArgb("#BBBBBB");

    public Color TextColor =>
        IsToday || IsCompleted                    ? Colors.White :
        IsFuture || (!IsEditable && !IsCompleted) ? Color.FromArgb("#555555") :
                                                    Color.FromArgb("#333333");

    // Divider between top/bottom halves — subtle contrast on any background
    public Color DividerColor =>
        IsToday || IsCompleted ? Color.FromArgb("#FFFFFF55") : Color.FromArgb("#00000022");
}

public partial class DashboardViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private string _myHabit = "";
    [ObservableProperty] private StatsResponse? _stats;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private double _wellFillPercent;
    [ObservableProperty] private int _currentStreak;
    [ObservableProperty] private bool _hasCourseDate;
    [ObservableProperty] private int _courseDayNumber = 1;  // actual day in the programme (date-based)

    public ObservableCollection<FeedItem> FeedItems { get; } = [];
    public ObservableCollection<CalendarDay> CalendarDays { get; } = [];

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var statsTask   = api.GetStatsAsync();
            var intakeTask  = api.GetIntakeAsync();
            var contentTask = api.GetCurrentWeekContentAsync();
            var logsTask    = api.GetLogsAsync();
            var configTask  = api.GetCourseStartDateAsync();
            var weeksTask   = api.GetAllWeeksAsync();

            await Task.WhenAll(
                statsTask.ContinueWith(_   => { }, TaskContinuationOptions.None),
                intakeTask.ContinueWith(_  => { }, TaskContinuationOptions.None),
                contentTask.ContinueWith(_ => { }, TaskContinuationOptions.None),
                logsTask.ContinueWith(_    => { }, TaskContinuationOptions.None),
                configTask.ContinueWith(_  => { }, TaskContinuationOptions.None),
                weeksTask.ContinueWith(_   => { }, TaskContinuationOptions.None));

            Stats         = statsTask.IsCompletedSuccessfully   ? statsTask.Result   : null;
            var intake    = intakeTask.IsCompletedSuccessfully  ? intakeTask.Result  : null;
            var content   = contentTask.IsCompletedSuccessfully ? contentTask.Result : null;
            var logs      = logsTask.IsCompletedSuccessfully    ? logsTask.Result    : [];
            var startDate = configTask.IsCompletedSuccessfully  ? configTask.Result  : null;
            var weeks     = weeksTask.IsCompletedSuccessfully   ? weeksTask.Result   : [];

            MyHabit = intake?.MyHabit ?? "";

            if (Stats is not null)
            {
                WellFillPercent = Stats.WellFillPercent;
                CurrentStreak   = Stats.CurrentStreak;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            // Day number = how far into the 60-day programme we are TODAY (date-based, not log-based)
            CourseDayNumber = startDate.HasValue
                ? Math.Max(1, Math.Min(today.DayNumber - startDate.Value.DayNumber + 1, 60))
                : 1;

            var completedCount = logs.Count(l => l.IsCompleted);

            // Build module title map: weekNumber → title
            var moduleTitles = weeks.ToDictionary(w => w.WeekNumber, w => w.ModuleTitle);

            // Feed
            FeedItems.Clear();
            FeedItems.Add(new FeedItem("Welcome", "Welcome back!", "Keep building your habit — every day counts."));
            FeedItems.Add(new FeedItem("Today's Events", $"Day {CourseDayNumber} of 60",
                completedCount > 0 ? $"You've completed {completedCount} day(s) so far." : "You haven't logged a completed day yet."));
            if (content is not null)
            {
                var courseDay = (CourseDayNumber % 7) + 1;
                if (!string.IsNullOrWhiteSpace(content.NotificationMessage) &&
                    (content.NotificationDay == 0 || content.NotificationDay == courseDay))
                    FeedItems.Add(new FeedItem("Notification", "Today's Reminder", content.NotificationMessage));

                FeedItems.Add(new FeedItem("Course",
                    $"Week {content.WeekNumber}: Module {content.ModuleNumber} — {content.ModuleTitle}",
                    "Tap to open this week's course material."));

                if (!string.IsNullOrWhiteSpace(content.MotivationalMessage))
                    FeedItems.Add(new FeedItem("This Week's Message", "Motivation", content.MotivationalMessage));
            }

            // Calendar — 8 weeks = 56 days from course start
            CalendarDays.Clear();
            HasCourseDate = startDate.HasValue;
            if (!startDate.HasValue) return;

            var completedDates = logs.Where(l => l.IsCompleted).Select(l => l.LogDate).ToHashSet();

            for (int i = 0; i < 56; i++)
            {
                var date     = startDate.Value.AddDays(i);
                var isFuture = date > today;
                var daysDiff = today.DayNumber - date.DayNumber;
                var isEditable = !isFuture && daysDiff <= 5;

                // Mondays: look up the module title for this week number
                string? moduleTitle = null;
                if (date.DayOfWeek == DayOfWeek.Monday)
                {
                    int weekNum = i / 7 + 1;
                    var title = moduleTitles.TryGetValue(weekNum, out var t) ? t : "";
                    moduleTitle = string.IsNullOrWhiteSpace(title)
                        ? $"Week {weekNum}"
                        : $"Week {weekNum}: {title}";
                }

                CalendarDays.Add(new CalendarDay
                {
                    Date        = date,
                    GoalDay     = i + 1,
                    IsCompleted = completedDates.Contains(date),
                    IsToday     = date == today,
                    IsFuture    = isFuture,
                    IsEditable  = isEditable,
                    ModuleTitle = moduleTitle
                });
            }
        }
        catch (Exception ex)
        {
            FeedItems.Clear();
            FeedItems.Add(new FeedItem("Error", "Could not load dashboard", ex.Message));
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task FeedItemTappedAsync(FeedItem item)
    {
        if (item.Type == "Course")
            await Shell.Current.GoToAsync("//CoursePage");
    }

    public async Task DayTappedAsync(CalendarDay day)
    {
        if (!day.IsEditable && !day.IsCompleted) return;
        await Shell.Current.GoToAsync(nameof(LogEntryPage),
            new Dictionary<string, object> { ["Date"] = day.Date.ToString("yyyy-MM-dd") });
    }
}
