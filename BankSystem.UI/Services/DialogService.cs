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
            vm.CloseAction = result =>
            {
                try
                {
                    dlg.Close(result);
                }
                catch
                {
                    dlg.Close();
                }
            };
            var dialogResult = await dlg.ShowDialog<bool?>(_mainWindow);
            return dialogResult ?? false;
        });
    }

    public async Task<T?> ShowListDialogAsync<T>(string title, string message, System.Collections.Generic.IEnumerable<T> items) where T : class
    {
        if (_mainWindow == null) return null;
        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            T? selectedItem = null;
            var listBox = new ListBox
            {
                ItemsSource = items,
                Margin = new Thickness(0, 12, 0, 12),
                Height = 200
            };

            var dialog = new Window
            {
                Title = title,
                Width = 450,
                Height = 350,
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Children =
                    {
                        new TextBlock{ Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8) },
                        listBox,
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                            Spacing = 8,
                            Children =
                            {
                                new Button{ Content = "Cancelar", Width = 100 },
                                new Button{ Content = "Seleccionar", Width = 100 }
                            }
                        }
                    }
                },
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (dialog.Content is StackPanel sp)
            {
                var buttonPanel = (StackPanel)sp.Children[2];
                var cancelBtn = (Button)buttonPanel.Children[0];
                var selectBtn = (Button)buttonPanel.Children[1];

                cancelBtn.Click += (_, _) => dialog.Close();
                selectBtn.Click += (_, _) =>
                {
                    selectedItem = listBox.SelectedItem as T;
                    dialog.Close();
                };

                listBox.DoubleTapped += (_, _) =>
                {
                    selectedItem = listBox.SelectedItem as T;
                    dialog.Close();
                };
            }

            await dialog.ShowDialog(_mainWindow);
            return selectedItem;
        });
    }

    public async Task<BankSystem.Domain.AccountBase?> ShowAccountSelectorAsync(string title, System.Collections.Generic.List<BankSystem.Domain.AccountBase> accounts)
    {
        if (_mainWindow == null || accounts == null || accounts.Count == 0) return null;
        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            BankSystem.Domain.AccountBase? selectedAccount = null;
            var scrollViewer = new ScrollViewer { MaxHeight = 400 };
            var stackPanel = new StackPanel { Spacing = 12, Margin = new Thickness(0, 12, 0, 12) };

            foreach (var account in accounts)
            {
                var accountType = account.GetType().Name.Replace("Account", "");
                var emoji = accountType.Contains("Checking") ? "💳" : accountType.Contains("Savings") ? "💰" : "📇";
                
                var card = new Border
                {
                    Background = Avalonia.Media.Brushes.White,
                    BorderBrush = Avalonia.Media.Brush.Parse("#E2E8F0"),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(16),
                    Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                    Tag = account
                };

                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto")
                };

                var iconBorder = new Border
                {
                    Background = Avalonia.Media.Brush.Parse("#DBEAFE"),
                    CornerRadius = new CornerRadius(8),
                    Width = 48,
                    Height = 48,
                    Padding = new Thickness(8)
                };
                iconBorder.Child = new TextBlock { Text = emoji, FontSize = 24, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(iconBorder, 0);

                var infoStack = new StackPanel { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Spacing = 4, Margin = new Thickness(12, 0, 0, 0) };
                infoStack.Children.Add(new TextBlock { Text = accountType, FontSize = 16, FontWeight = Avalonia.Media.FontWeight.Bold, Foreground = Avalonia.Media.Brush.Parse("#1E293B") });
                infoStack.Children.Add(new TextBlock { Text = $"N°: {account.Number}", FontSize = 12, Foreground = Avalonia.Media.Brush.Parse("#64748B") });
                Grid.SetColumn(infoStack, 1);

                var balanceText = new TextBlock { Text = $"{account.Balance:C}", FontSize = 18, FontWeight = Avalonia.Media.FontWeight.Bold, Foreground = Avalonia.Media.Brush.Parse("#FF6600"), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(balanceText, 2);

                grid.Children.Add(iconBorder);
                grid.Children.Add(infoStack);
                grid.Children.Add(balanceText);
                card.Child = grid;

                card.PointerPressed += (s, e) =>
                {
                    selectedAccount = (s as Border)?.Tag as BankSystem.Domain.AccountBase;
                    foreach (var child in stackPanel.Children)
                    {
                        if (child is Border b) b.BorderBrush = Avalonia.Media.Brush.Parse("#E2E8F0");
                    }
                    if (s is Border selectedBorder) selectedBorder.BorderBrush = Avalonia.Media.Brush.Parse("#FF6600");
                };

                stackPanel.Children.Add(card);
            }

            scrollViewer.Content = stackPanel;

            var dialog = new Window
            {
                Title = title,
                Width = 500,
                Height = 550,
                Content = new StackPanel
                {
                    Margin = new Thickness(20),
                    Children =
                    {
                        new TextBlock{ Text = "Selecciona una cuenta:", FontSize = 14, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Thickness(0, 0, 0, 8) },
                        scrollViewer,
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                            Spacing = 12,
                            Margin = new Thickness(0, 16, 0, 0),
                            Children =
                            {
                                new Button{ Content = "Cancelar", Width = 120, Height = 40 },
                                new Button{ Content = "Confirmar", Width = 120, Height = 40, Background = Avalonia.Media.Brush.Parse("#FF6600"), Foreground = Avalonia.Media.Brushes.White }
                            }
                        }
                    }
                },
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (dialog.Content is StackPanel sp)
            {
                var buttonPanel = (StackPanel)sp.Children[2];
                var cancelBtn = (Button)buttonPanel.Children[0];
                var confirmBtn = (Button)buttonPanel.Children[1];

                cancelBtn.Click += (_, _) => dialog.Close();
                confirmBtn.Click += (_, _) => dialog.Close();
            }

            await dialog.ShowDialog(_mainWindow);
            return selectedAccount;
        });
    }
}
