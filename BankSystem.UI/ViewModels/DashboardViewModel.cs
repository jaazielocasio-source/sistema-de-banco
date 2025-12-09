using BankSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BankSystem.UI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly BankService _bank;
    private readonly SchedulerService _scheduler;
    private readonly Services.DialogService _dialogs;

    [ObservableProperty] private decimal _totalBalance;
    [ObservableProperty] private int _accountCount;
    public ObservableCollection<string> UpcomingPayments { get; } = new();

    public DashboardViewModel(BankService bank, SchedulerService scheduler, Services.DialogService dialogs)
    {
        _bank = bank;
        _scheduler = scheduler;
        _dialogs = dialogs;
        
        // Inicializar con valores por defecto
        _totalBalance = 0m;
        _accountCount = 0;
        
        Refresh();
    }

    [RelayCommand]
    public void Refresh()
    {
        try
        {
            // Calcular saldo total con manejo de errores
            TotalBalance = _bank.Accounts?.Sum(a => a?.Balance ?? 0m) ?? 0m;
            AccountCount = _bank.Accounts?.Count ?? 0;
            
            UpcomingPayments.Clear();
            
            if (_bank.ScheduledPayments != null)
            {
                foreach (var p in _bank.ScheduledPayments.OrderBy(p => p.NextDate).Take(5))
                {
                    UpcomingPayments.Add($"{p.NextDate:yyyy-MM-dd}: {p.SourceAccount}->{p.DestinationId} {p.Amount:C} ({p.Periodicity})");
                }
            }
            
            // Si no hay pagos programados, mostrar mensaje
            if (UpcomingPayments.Count == 0)
            {
                UpcomingPayments.Add("No hay pagos automáticos programados");
            }
        }
        catch (Exception ex)
        {
            TotalBalance = 0m;
            AccountCount = 0;
            UpcomingPayments.Clear();
            UpcomingPayments.Add($"Error al cargar datos: {ex.Message}");
        }
    }
}
