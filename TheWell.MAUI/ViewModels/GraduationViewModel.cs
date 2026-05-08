using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TheWell.Core.DTOs;
using TheWell.MAUI.Services;

namespace TheWell.MAUI.ViewModels;

public partial class GraduationViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private StatsResponse? _stats;
    [ObservableProperty] private bool _isBusy;

    public ObservableCollection<DailyLogResponse> Logs { get; } = [];

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var statsTask = api.GetStatsAsync();
            var logsTask = api.GetLogsAsync();
            await Task.WhenAll(statsTask, logsTask);

            Stats = await statsTask;
            Logs.Clear();
            foreach (var log in await logsTask)
                Logs.Add(log);
        }
        finally { IsBusy = false; }
    }
}
