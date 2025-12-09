using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using BankSystem.UI.ViewModels;
using BankSystem.UI.Views;

namespace BankSystem.UI.Services;

public class DialogService
{
    private Window? _mainWindow;

    public void RegisterWindow(Window window) => _mainWindow = window;

    public async Task ShowMessageAsync(string title, string message)
    {
        if (_mainWindow == null) return;
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var dialog = new Window
            {
                Title = title,
                Width = 360,
                Height = 180,
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Children =
                    {
                        new TextBlock{ Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                        new Button{ Content = "Cerrar", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Margin = new Thickness(0,12,0,0)}
                    }
                },
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if (dialog.Content is StackPanel sp && sp.Children[1] is Button btn)
            {
                btn.Click += (_, _) => dialog.Close();
            }
            await dialog.ShowDialog(_mainWindow);
        });
    }

    public Task ShowErrorAsync(string message) => ShowMessageAsync("Error", message);

    public async Task<bool> ShowTransferDialogAsync(TransferDialogViewModel vm)
    {
        if (_mainWindow == null) return false;
        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var dlg = new TransferDialog { DataContext = vm };
            vm.CloseAction = result => dlg.Close(result);
            return await dlg.ShowDialog<bool>(_mainWindow);
        });
    }
}
