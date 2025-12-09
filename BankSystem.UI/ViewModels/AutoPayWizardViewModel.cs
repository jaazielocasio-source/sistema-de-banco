using BankSystem.Domain;
using BankSystem.Services;
using BankSystem.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BankSystem.UI.ViewModels;

public partial class AutoPayWizardViewModel : ObservableObject
{
    private readonly BankService _bank;
    private readonly DialogService _dialogs;

    public ObservableCollection<string> Accounts { get; } = new();
    public Periodicity[] PeriodicityOptions { get; } = new[] { Periodicity.Monthly, Periodicity.Daily };
    public Action? OnSaved { get; set; }

    [ObservableProperty] private string? _sourceAccount;
    [ObservableProperty] private string? _destinationId;
    [ObservableProperty] private string _name = "AutoPay";
    [ObservableProperty] private string? _memo;
    [ObservableProperty] private decimal _amount = 50;
    [ObservableProperty] private Periodicity _periodicity = Periodicity.Monthly;
    [ObservableProperty] private int _dayOfMonth = 15;
    [ObservableProperty] private DateTime _startDate = DateTime.Today.AddDays(1);
    [ObservableProperty] private DateTime? _endDate;
    [ObservableProperty] private int _maxRetries = 1;
    [ObservableProperty] private int _retryEveryDays = 2;
    [ObservableProperty] private string? _status;

    public AutoPayWizardViewModel(BankService bank, DialogService dialogs)
    {
        _bank = bank;
        _dialogs = dialogs;
        foreach (var a in bank.Accounts) Accounts.Add(a.Number);
    }

    partial void OnSourceAccountChanged(string? value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnDestinationIdChanged(string? value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnAmountChanged(decimal value) => SaveCommand.NotifyCanExecuteChanged();

    private bool CanSave() => !string.IsNullOrWhiteSpace(SourceAccount) && !string.IsNullOrWhiteSpace(DestinationId) && Amount > 0;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!CanSave()) return;
        var ok = _bank.SchedulePayment(SourceAccount!, DestinationId!, Amount, Periodicity, DayOfMonth, StartDate, EndDate, MaxRetries, RetryEveryDays, Name, Memo);
        if (!ok)
        {
            Status = "No se pudo programar";
            await _dialogs.ShowErrorAsync("No se pudo programar el pago automático.");
            return;
        }
        Status = "Programado";
        await _dialogs.ShowMessageAsync("AutoPay", "Pago automático creado.");
        OnSaved?.Invoke();
    }
}
