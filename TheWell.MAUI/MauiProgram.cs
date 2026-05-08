using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using TheWell.MAUI.Services;
using TheWell.MAUI.ViewModels;
using TheWell.MAUI.Views;

namespace TheWell.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // HTTP client pointing at the API
        // 10.0.2.2 routes to the host's localhost inside the Android emulator.
        // On Windows/iOS we use localhost directly.
        builder.Services.AddHttpClient<ApiService>(client =>
        {
#if ANDROID
            client.BaseAddress = new Uri("http://10.0.2.2:5139/");
#else
            client.BaseAddress = new Uri("http://localhost:5139/");
#endif
        });

        // WordPress content (direct — no auth required)
        builder.Services.AddHttpClient<WordPressContentService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ForceResetViewModel>();
        builder.Services.AddTransient<IntakeViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<LogEntryViewModel>();
        builder.Services.AddTransient<GraduationViewModel>();
        builder.Services.AddTransient<GoalViewModel>();
        builder.Services.AddTransient<CourseViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ForceResetPage>();
        builder.Services.AddTransient<IntakePage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<LogEntryPage>();
        builder.Services.AddTransient<GraduationPage>();
        builder.Services.AddTransient<GoalPage>();
        builder.Services.AddTransient<CoursePage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
