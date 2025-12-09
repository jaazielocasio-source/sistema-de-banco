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

/// <summary>
/// ViewModel para gestionar pagos automáticos y recurrentes
/// Permite programar, ejecutar y cancelar pagos automáticos
/// </summary>
public partial class AutoPayViewModel : ObservableObject
{
    // Servicios de negocio
    private readonly BankService _bank;
    private readonly SchedulerService _scheduler;
    private readonly DialogService _dialogs;

    // Colección observable de pagos programados
    public ObservableCollection<PaymentSchedule> Payments { get; } = new();
    
    // Wizard para crear nuevos pagos automáticos
    public AutoPayWizardViewModel Wizard { get; }

    // Pago seleccionado de la tabla
    [ObservableProperty] private PaymentSchedule? _selectedPayment;
    
    // Mensaje de estado de las operaciones
    [ObservableProperty] private string? _status;

    /// <summary>
    /// Constructor: inicializa servicios y carga pagos existentes
    /// </summary>
    public AutoPayViewModel(BankService bank, SchedulerService scheduler, DialogService dialogs)
    {
        _bank = bank;
        _scheduler = scheduler;
        _dialogs = dialogs;
        
        // Inicializar wizard y vincular su evento de guardado
        Wizard = new AutoPayWizardViewModel(bank, dialogs);
        Wizard.OnSaved = Refresh;
        
        // Cargar pagos programados existentes
        Refresh();
    }

    /// <summary>
    /// Refresca la lista de pagos programados desde el servicio
    /// </summary>
    public void Refresh()
    {
        var selected = SelectedPayment?.SourceAccount; // Guardar selección actual
        Payments.Clear();
        
        // Cargar todos los pagos ordenados por fecha próxima
        foreach (var p in _bank.ScheduledPayments.OrderBy(p => p.NextDate)) 
        {
            Payments.Add(p);
        }
        
        // Restaurar selección si existe, sino seleccionar el primero
        SelectedPayment = Payments.FirstOrDefault(p => p.SourceAccount == selected) ?? Payments.FirstOrDefault();
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
