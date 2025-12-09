using BankSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BankSystem.UI.ViewModels;

/// <summary>
/// ViewModel del Dashboard principal
/// Muestra resumen de cuentas, saldo y próximos pagos
/// Permite seleccionar una cuenta específica para ver su detalle
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    // Servicios de negocio
    private readonly BankService _bank;
    private readonly SchedulerService _scheduler;
    private readonly Services.DialogService _dialogs;

    // Saldo total o de cuenta seleccionada
    [ObservableProperty] private decimal _totalBalance;
    
    // Cantidad total de cuentas
    [ObservableProperty] private int _accountCount;
    
    // Cuenta actualmente seleccionada para mostrar en el dashboard
    [ObservableProperty] private BankSystem.Domain.AccountBase? _selectedAccount;
    
    // Número de cuenta mostrado en la tarjeta
    [ObservableProperty] private string _accountNumber = "Ninguna seleccionada";
    
    // Tipo de cuenta seleccionada
    [ObservableProperty] private string _accountType = "";
    
    // Lista de próximos pagos automáticos
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
            // Si hay una cuenta seleccionada, mostrar su saldo, sino mostrar el total
            if (SelectedAccount != null)
            {
                TotalBalance = SelectedAccount.Balance;
                AccountNumber = SelectedAccount.Number;
                AccountType = SelectedAccount.GetType().Name.Replace("Account", "");
            }
            else
            {
                // Calcular saldo total con manejo de errores
                TotalBalance = _bank.Accounts?.Sum(a => a?.Balance ?? 0m) ?? 0m;
                AccountNumber = "Todas las cuentas";
                AccountType = "Total";
            }
            
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

    [RelayCommand]
    private async System.Threading.Tasks.Task TransferAsync()
    {
        try
        {
            var vm = new TransferDialogViewModel(_bank, _dialogs);
            var result = await _dialogs.ShowTransferDialogAsync(vm);
            if (result)
            {
                Refresh(); // Actualizar el dashboard después de la transferencia
            }
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync($"Error al abrir transferencia: {ex.Message}");
        }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task SendMoneyAsync()
    {
        // Abrir el diálogo de transferencia
        try
        {
            var vm = new TransferDialogViewModel(_bank, _dialogs);
            var result = await _dialogs.ShowTransferDialogAsync(vm);
            if (result)
            {
                Refresh();
            }
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync($"Error al abrir envío de dinero: {ex.Message}");
        }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ViewCardsAsync()
    {
        if (_bank.Accounts == null || _bank.Accounts.Count == 0)
        {
            await _dialogs.ShowMessageAsync(
                "💳 Ver Tarjetas", 
                "No tienes cuentas/tarjetas activas.\n\nVe a la sección '💰 Cuentas' para crear una."
            );
            return;
        }

        var account = await _dialogs.ShowAccountSelectorAsync("💳 Selecciona una Tarjeta para el Dashboard", _bank.Accounts);
        
        if (account != null)
        {
            // Actualizar la cuenta seleccionada y refrescar el dashboard
            SelectedAccount = account;
            Refresh();
            
            var accountType = account.GetType().Name.Replace("Account", "");
            var details = $"✅ Dashboard actualizado\n\n";
            details += $"Ahora mostrando información de:\n\n";
            details += $"📋 {accountType}\n";
            details += $"📇 Número: {account.Number}\n";
            details += $"💰 Saldo: {account.Balance:C}\n";
            details += $"📊 Estado: {(account.Status == BankSystem.Domain.AccountStatus.Active ? "Activa ✅" : "Congelada ❄️")}\n";
            details += $"💵 Moneda: {account.Currency}\n\n";
            
            if (account is BankSystem.Domain.CheckingAccount checking)
            {
                details += $"💳 Límite de sobregiro: {checking.OverdraftLimit:C}\n";
            }
            else if (account is BankSystem.Domain.SavingsAccount savings)
            {
                details += $"💰 Retiros disponibles: {BankSystem.Domain.FeeSchedule.SavingsMonthlyWithdrawalLimit - savings.WithdrawalsThisMonth}\n";
            }
            
            details += $"\n💡 Para ver todas las cuentas de nuevo, haz clic en 'Ver tarjetas' y selecciona una cuenta diferente.";

            await _dialogs.ShowMessageAsync($"💳 {accountType} Seleccionada", details);
        }
    }
}
