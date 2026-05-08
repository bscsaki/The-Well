using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TheWell.Core.DTOs;
using TheWell.MAUI.Services;

namespace TheWell.MAUI.ViewModels;

public partial class WeekItem : ObservableObject
{
    public int WeekNumber { get; init; }
    public int ModuleNumber { get; init; }
    public string ModuleTitle { get; init; } = "";
    public bool IsLocked { get; init; }

    public string Label => $"Week {WeekNumber}: Module {ModuleNumber}";
    public string SubLabel => string.IsNullOrWhiteSpace(ModuleTitle) ? "(No title)" : ModuleTitle;
    public Color CardColor => IsLocked ? Color.FromArgb("#D0D0D0") : Color.FromArgb("#1A6B8A");
    public Color TextColor => IsLocked ? Color.FromArgb("#888888") : Colors.White;
    public string LockStatus => IsLocked ? "Locked" : "Open";
}

public partial class CourseViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _courseNotStarted;

    // Week list view
    public ObservableCollection<WeekItem> Weeks { get; } = [];

    // Week detail view
    [ObservableProperty] private bool _showingDetail;
    [ObservableProperty] private bool _hasSelectedContent;
    [ObservableProperty] private WeeklyContentResponse? _selectedContent;
    [ObservableProperty] private string _detailTitle = "";

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = "";
        ShowingDetail = false;
        SelectedContent = null;
        HasSelectedContent = false;
        CourseNotStarted = false;
        try
        {
            var weeks = await api.GetAllWeeksAsync();
            Weeks.Clear();
            if (weeks.Count == 0)
            {
                CourseNotStarted = true;
                return;
            }
            foreach (var w in weeks)
                Weeks.Add(new WeekItem
                {
                    WeekNumber = w.WeekNumber,
                    ModuleNumber = w.ModuleNumber,
                    ModuleTitle = w.ModuleTitle,
                    IsLocked = w.IsLocked
                });
        }
        catch { ErrorMessage = "Could not load course content."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task OpenWeekAsync(WeekItem week)
    {
        if (week.IsLocked) return;

        IsBusy = true;
        ErrorMessage = "";
        try
        {
            var content = await api.GetWeekContentAsync(week.WeekNumber);
            if (content is null)
            {
                ErrorMessage = "Could not load week content.";
                return;
            }
            SelectedContent = content;
            HasSelectedContent = true;
            DetailTitle = $"Week {content.WeekNumber}: {content.ModuleTitle}";
            ShowingDetail = true;
        }
        catch { ErrorMessage = "Could not load week content."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public void BackToList()
    {
        ShowingDetail = false;
        SelectedContent = null;
        HasSelectedContent = false;
    }
}
