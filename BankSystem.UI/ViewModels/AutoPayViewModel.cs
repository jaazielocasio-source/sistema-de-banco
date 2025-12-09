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

public partial class AutoPayViewModel : ObservableObject
{
    private readonly BankService _bank;
    private readonly SchedulerService _scheduler;
    private readonly DialogService _dialogs;

    public ObservableCollection<PaymentSchedule> Payments { get; } = new();
    public AutoPayWizardViewModel Wizard { get; }

    [ObservableProperty] private PaymentSchedule? _selectedPayment;
    [ObservableProperty] private string? _status;

    public AutoPayViewModel(BankService bank, SchedulerService scheduler, DialogService dialogs)
    {
        _bank = bank;
        _scheduler = scheduler;
        _dialogs = dialogs;
        Wizard = new AutoPayWizardViewModel(bank, dialogs);
        Wizard.OnSaved = Refresh;
        Refresh();
    }

    public void Refresh()
    {
        Payments.Clear();
        foreach (var p in _bank.ScheduledPayments.OrderBy(p => p.NextDate)) Payments.Add(p);
        SelectedPayment = Payments.FirstOrDefault();
    }

    partial void OnSelectedPaymentChanged(PaymentSchedule? value)
    {
        CancelCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task ExecuteTodayAsync()
    {
        var processed = _scheduler.ExecuteDuePayments(_bank, DateTime.Today);
        Status = $"Procesados: {processed}";
        if (processed == 0) await _dialogs.ShowMessageAsync("AutoPay", "No hay pagos para hoy o fueron pospuestos.");
        Refresh();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task CancelAsync()
    {
        if (SelectedPayment == null) return;
        SelectedPayment.Active = false;
        Status = "Pago automático cancelado";
        await _dialogs.ShowMessageAsync("AutoPay", "Pago automático cancelado.");
        Refresh();
    }

    private bool HasSelection() => SelectedPayment != null;
}
