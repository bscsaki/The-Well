using TheWell.MAUI.Views;

namespace TheWell.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ForceResetPage), typeof(ForceResetPage));
        Routing.RegisterRoute(nameof(LogEntryPage), typeof(LogEntryPage));

        this.Navigated += OnShellNavigated;
    }

    private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        var loc = e.Current?.Location?.ToString() ?? "";

        HomeTab.Title    = loc.Contains("DashboardPage") ? "W  Well"     : "W";
        HabitTab.Title   = loc.Contains("HabitPage")     ? "H  Habit"    : "H";
        CourseTab.Title  = loc.Contains("CoursePage")    ? "C  Course"   : "C";
        SettingsTab.Title = loc.Contains("SettingsPage") ? "S  Settings" : "S";
    }
}
