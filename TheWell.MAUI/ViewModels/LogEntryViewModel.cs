using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TheWell.Core.DTOs;
using TheWell.MAUI.Services;

namespace TheWell.MAUI.ViewModels;

[QueryProperty(nameof(DateString), "Date")]
public partial class LogEntryViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private string _dateString = "";
    [ObservableProperty] private bool _isCompleted;
    [ObservableProperty] private string _note = "";
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _alreadyLocked;

    private Guid? _existingLogId;
    public DateOnly LogDate { get; private set; }
    public string DateDisplay { get; private set; } = "";
    public event Action? ConfettiRequested;

    public async Task InitAsync()
    {
        LogDate = string.IsNullOrEmpty(DateString)
            ? DateOnly.FromDateTime(DateTime.Today)
            : DateOnly.Parse(DateString);
        DateDisplay = LogDate.ToString("dddd, MMMM d");

        IsBusy = true;
        try
        {
            var logs = await api.GetLogsAsync();
            var existing = logs.FirstOrDefault(l => l.LogDate == LogDate);
            if (existing is not null)
            {
                _existingLogId = existing.LogID;
                IsCompleted = existing.IsCompleted;
                Note = existing.Note ?? "";
                AlreadyLocked = existing.IsLocked;
                if (existing.IsCompleted)
                    StatusMessage = "You already completed this day ✓";
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (AlreadyLocked) { StatusMessage = "This day is locked and cannot be edited."; return; }

        if (!IsCompleted && !_existingLogId.HasValue)
        {
            StatusMessage = "Toggle on to record that you completed your goal today.";
            return;
        }

        IsBusy = true;
        StatusMessage = "";
        try
        {
            DailyLogResponse? result;
            if (_existingLogId.HasValue)
                result = await api.UpdateLogAsync(_existingLogId.Value, new UpdateLogRequest(IsCompleted, Note));
            else
                result = await api.CreateLogAsync(new CreateLogRequest(LogDate, IsCompleted, Note));

            if (result is null) { StatusMessage = "Could not save log."; return; }
            if (IsCompleted) ConfettiRequested?.Invoke();
            StatusMessage = "Log saved!";
            await Task.Delay(1200);
            await Shell.Current.GoToAsync("..");
        }
        finally { IsBusy = false; }
    }
}
