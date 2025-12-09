using BankSystem.Services;
using BankSystem.Domain;
using BankSystem.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BankSystem.UI.ViewModels;

/// <summary>
/// ViewModel para gestión de cuentas bancarias
/// Permite crear cuentas, depositar, retirar, transferir y cambiar estados
/// </summary>
public partial class AccountsViewModel : ObservableObject
{
    // Servicios de negocio
    private readonly BankService _bank;
    private readonly DialogService _dialogs;

    // Colección observable de todas las cuentas
    public ObservableCollection<AccountItem> Accounts { get; } = new();

    // Cuenta seleccionada para operaciones
    [ObservableProperty] private AccountItem? _selectedAccount;
    
    // Monto para operaciones de depósito/retiro
    [ObservableProperty] private decimal _amount = 100;
    
    // Mensaje de estado de operaciones
    [ObservableProperty] private string? _status;
    
    // Campos para crear nueva cuenta
    [ObservableProperty] private int _newCustomerId = 1;
    [ObservableProperty] private int _newAccountTypeIndex = 0; // 0=Ahorro, 1=Corriente, 2=CD
    [ObservableProperty] private int _newCurrencyIndex = 0; // 0=USD, 1=EUR, 2=GBP

    public AccountsViewModel(BankService bank, DialogService dialogs)
    {
        _bank = bank;
        _dialogs = dialogs;
        Refresh();
    }

    public void Refresh()
    {
        var selected = SelectedAccount?.Number;
        Accounts.Clear();
        foreach (var a in _bank.Accounts)
        {
            Accounts.Add(new AccountItem(a.Number, a.GetType().Name, a.Currency, a.Balance, a.Status, a.CustomerId));
        }
        SelectedAccount = Accounts.FirstOrDefault(a => a.Number == selected) ?? Accounts.FirstOrDefault();
        DepositCommand.NotifyCanExecuteChanged();
        WithdrawCommand.NotifyCanExecuteChanged();
        FreezeCommand.NotifyCanExecuteChanged();
        UnfreezeCommand.NotifyCanExecuteChanged();
        TransferCommand.NotifyCanExecuteChanged();
    }

    partial void OnAmountChanged(decimal value)
    {
        DepositCommand.NotifyCanExecuteChanged();
        WithdrawCommand.NotifyCanExecuteChanged();
        TransferCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedAccountChanged(AccountItem? value)
    {
        DepositCommand.NotifyCanExecuteChanged();
        WithdrawCommand.NotifyCanExecuteChanged();
        FreezeCommand.NotifyCanExecuteChanged();
        UnfreezeCommand.NotifyCanExecuteChanged();
        TransferCommand.NotifyCanExecuteChanged();
    }

    private bool HasAccount() => SelectedAccount != null && Amount > 0;

    [RelayCommand]
    private async Task DepositAsync()
    {
        try
        {
            if (_bank.Accounts == null || _bank.Accounts.Count == 0)
            {
                await _dialogs.ShowErrorAsync("No tienes cuentas disponibles.\n\nCrea una cuenta primero.");
                return;
            }

            var account = await _dialogs.ShowAccountSelectorAsync("💰 Depositar - Selecciona la cuenta", _bank.Accounts);
            
            if (account != null)
            {
                var ok = _bank.Deposit(account.Number, Amount);
                Status = ok ? $"✅ Depósito de {Amount:C} exitoso en {account.Number}" : "❌ Depósito falló";
                if (!ok) 
                {
                    await _dialogs.ShowErrorAsync("No se pudo depositar.");
                }
                else
                {
                    await _dialogs.ShowMessageAsync("✅ Depósito Exitoso", $"Se depositaron {Amount:C} en la cuenta {account.Number}");
                }
                Refresh();
            }
        }
        catch (Exception ex)
        {
            Status = $"❌ Error: {ex.Message}";
            await _dialogs.ShowErrorAsync($"Error al depositar: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(HasAccount))]
    private async Task WithdrawAsync()
    {
        try
        {
            if (SelectedAccount == null) return;
            var ok = _bank.Withdraw(SelectedAccount.Number, Amount);
            Status = ok ? "✅ Retiro exitoso" : "❌ Retiro falló";
            if (!ok) await _dialogs.ShowErrorAsync("No se pudo retirar (fondos, estado o límite).");
            Refresh();
        }
        catch (Exception ex)
        {
            Status = $"❌ Error: {ex.Message}";
            await _dialogs.ShowErrorAsync($"Error al retirar: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(HasAccount))]
    private async Task TransferAsync()
    {
        try
        {
            if (SelectedAccount == null) return;
            var vm = new TransferDialogViewModel(_bank, _dialogs)
            {
                SourceAccount = SelectedAccount.Number
            };
            var ok = await _dialogs.ShowTransferDialogAsync(vm);
            if (ok)
            {
                Status = "✅ Transferencia completada";
                Refresh();
            }
            else
            {
                Status = "❌ Transferencia cancelada";
            }
        }
        catch (Exception ex)
        {
            Status = $"❌ Error: {ex.Message}";
            await _dialogs.ShowErrorAsync($"Error en transferencia: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(HasAccount))]
    private async Task FreezeAsync()
    {
        try
        {
            if (SelectedAccount == null) return;
            var ok = _bank.FreezeAccount(SelectedAccount.Number);
            Status = ok ? "🔒 Tarjeta desactivada" : "❌ No se pudo desactivar";
            if (!ok) await _dialogs.ShowErrorAsync("No se pudo congelar la cuenta.");
            Refresh();
        }
        catch (Exception ex)
        {
            Status = $"❌ Error: {ex.Message}";
            await _dialogs.ShowErrorAsync($"Error al desactivar: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(HasAccount))]
    private async Task UnfreezeAsync()
    {
        try
        {
            if (SelectedAccount == null) return;
            var ok = _bank.UnfreezeAccount(SelectedAccount.Number);
            Status = ok ? "🔓 Tarjeta activada" : "❌ No se pudo activar";
            if (!ok) await _dialogs.ShowErrorAsync("No se pudo activar la cuenta.");
            Refresh();
        }
        catch (Exception ex)
        {
            Status = $"❌ Error: {ex.Message}";
            await _dialogs.ShowErrorAsync($"Error al activar: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CreateAccountAsync()
    {
        try
        {
            if (NewCustomerId <= 0)
            {
                await _dialogs.ShowErrorAsync("❌ ID de cliente inválido.\n\nDebe ser mayor a 0.");
                return;
            }

            // Convertir índices a valores
            var accountType = (NewAccountTypeIndex + 1).ToString(); // 0->1, 1->2, 2->3
            var currencies = new[] { "USD", "EUR", "GBP" };
            var currency = currencies[NewCurrencyIndex];

            var account = _bank.CreateAccount(NewCustomerId, accountType, currency);
            if (account == null)
            {
                await _dialogs.ShowErrorAsync("❌ No se pudo crear la cuenta.\n\nVerifica el tipo de cuenta seleccionado.");
                return;
            }

            var accountTypeNames = new[] { "Ahorro", "Corriente", "Certificado de Depósito" };
            var accountTypeName = accountTypeNames[NewAccountTypeIndex];
            Status = $"✅ Cuenta {accountTypeName} creada: {account.Number}";
            
            await _dialogs.ShowMessageAsync(
                "✅ Cuenta Creada",
                $"Se ha creado exitosamente una cuenta de {accountTypeName}:\n\n" +
                $"📇 Número: {account.Number}\n" +
                $"💰 Saldo inicial: {account.Balance:C}\n" +
                $"💵 Moneda: {account.Currency}\n\n" +
                $"La cuenta ya está visible en la lista de tarjetas."
            );
            
            Refresh();
        }
        catch (Exception ex)
        {
            Status = $"❌ Error: {ex.Message}";
            await _dialogs.ShowErrorAsync($"Error al crear cuenta: {ex.Message}");
        }
    }
}

public record AccountItem(string Number, string Type, string Currency, decimal Balance, AccountStatus Status, int CustomerId);
