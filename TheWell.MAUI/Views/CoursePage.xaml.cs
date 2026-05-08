using TheWell.MAUI.ViewModels;

namespace TheWell.MAUI.Views;

public partial class CoursePage : ContentPage
{
    private readonly CourseViewModel _vm;

    public CoursePage(CourseViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _vm.PropertyChanged += OnVmPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }

    private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CourseViewModel.SelectedContent) && _vm.SelectedContent is not null)
        {
            var html = _vm.SelectedContent.CourseMaterial;
            if (!string.IsNullOrWhiteSpace(html))
            {
                MaterialWebView.Source = new HtmlWebViewSource
                {
                    Html = $"<html><body style='font-family:sans-serif;padding:16px;font-size:15px;line-height:1.6;'>{html}</body></html>"
                };
            }
        }
    }
}
