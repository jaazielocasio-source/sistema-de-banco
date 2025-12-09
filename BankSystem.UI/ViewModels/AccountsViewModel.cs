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

public partial class AccountsViewModel : ObservableObject
{
    private readonly BankService _bank;
    private readonly DialogService _dialogs;

    public ObservableCollection<AccountItem> Accounts { get; } = new();

    [ObservableProperty] private AccountItem? _selectedAccount;
    [ObservableProperty] private decimal _amount = 100;
    [ObservableProperty] private string? _status;

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

    [RelayCommand(CanExecute = nameof(HasAccount))]
    private async Task DepositAsync()
    {
        try
        {
            if (SelectedAccount == null) return;
            var ok = _bank.Deposit(SelectedAccount.Number, Amount);
            Status = ok ? "✅ Depósito exitoso" : "❌ Depósito falló";
            if (!ok) await _dialogs.ShowErrorAsync("No se pudo depositar.");
            Refresh();
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
}

public record AccountItem(string Number, string Type, string Currency, decimal Balance, AccountStatus Status, int CustomerId);
