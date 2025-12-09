using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BankSystem.UI.Services;
using BankSystem.UI.ViewModels;
using BankSystem.UI.Views;

namespace BankSystem.UI;

public partial class App : Application
{
    private UiBootstrap? _bootstrap;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _bootstrap = new UiBootstrap();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainVm = _bootstrap.MainWindowViewModel;
            var mainWindow = new MainWindow { DataContext = mainVm };
            _bootstrap.Dialogs.RegisterWindow(mainWindow);
            desktop.MainWindow = mainWindow;
        }
        base.OnFrameworkInitializationCompleted();
    }
}
