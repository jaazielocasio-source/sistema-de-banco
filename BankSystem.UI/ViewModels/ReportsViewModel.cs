using BankSystem.Services;
using BankSystem.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BankSystem.UI.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly BankService _bank;
    private readonly DialogService _dialogs;

    public ObservableCollection<string> Accounts { get; } = new();

    [ObservableProperty] private string? _selectedAccount;
    [ObservableProperty] private int _month = DateTime.Today.Month;
    [ObservableProperty] private int _year = DateTime.Today.Year;
    [ObservableProperty] private string? _lastMessage;

    public ReportsViewModel(BankService bank, DialogService dialogs)
    {
        _bank = bank;
        _dialogs = dialogs;
        foreach (var a in bank.Accounts) Accounts.Add(a.Number);
        SelectedAccount = Accounts.FirstOrDefault();
    }

    partial void OnSelectedAccountChanged(string? value)
    {
        ExportCsvCommand.NotifyCanExecuteChanged();
        ExportPdfCommand.NotifyCanExecuteChanged();
        SendEStatementCommand.NotifyCanExecuteChanged();
    }

    private bool CanRun() => !string.IsNullOrWhiteSpace(SelectedAccount);

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task ExportCsvAsync()
    {
        var path = ReportService.ExportMonthlyStatementCSV(_bank, SelectedAccount!, Month, Year);
        LastMessage = path ?? "Cuenta no encontrada";
        if (path == null) await _dialogs.ShowErrorAsync("Cuenta no encontrada.");
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task ExportPdfAsync()
    {
        var path = ReportService.ExportMonthlyStatementPDF(_bank, SelectedAccount!, Month, Year);
        LastMessage = path ?? "Cuenta no encontrada";
        if (path == null) await _dialogs.ShowErrorAsync("Cuenta no encontrada.");
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task SendEStatementAsync()
    {
        var pdf = ReportService.ExportMonthlyStatementPDF(_bank, SelectedAccount!, Month, Year);
        if (pdf == null)
        {
            await _dialogs.ShowErrorAsync("Cuenta no encontrada.");
            return;
        }
        var sent = ReportService.SendMonthlyEStatement(_bank, SelectedAccount!, Month, Year, pdf);
        LastMessage = sent ? $"Enviado: {pdf}" : "Falló envío";
        if (!sent) await _dialogs.ShowErrorAsync("No se pudo enviar.");
    }
}
