using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EF_1.Desktop.Services;
using EF_1.Desktop.ViewModels;
using EF_1.Desktop.Views;

namespace EF_1.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();

            window.DataContext = new MainViewModel(new DialogService(window));

            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}