using BankSystem.Domain;
using BankSystem.Services;
using BankSystem.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BankSystem.UI.ViewModels;

public partial class AdminViewModel : ObservableObject
{
    private readonly BankService _bank;
    private readonly DialogService _dialogs;

    public AccountStatus[] StatusOptions { get; } = new[] { AccountStatus.Active, AccountStatus.Closed, AccountStatus.Frozen };

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _govId = string.Empty;

    [ObservableProperty] private int _customerIdForAccount;
    [ObservableProperty] private string _accountType = "1";
    [ObservableProperty] private string _accountCurrency = "USD";

    [ObservableProperty] private string _accountNumberStatus = string.Empty;
    [ObservableProperty] private AccountStatus _newStatus = AccountStatus.Active;

    [ObservableProperty] private string? _statusMessage;

    public AdminViewModel(BankService bank, DialogService dialogs)
    {
        _bank = bank;
        _dialogs = dialogs;
    }

    [RelayCommand]
    private async Task CreateCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            await _dialogs.ShowErrorAsync("Email o nombre inválido.");
            return;
        }
        if (!Regex.IsMatch(Phone ?? string.Empty, @"^[0-9]{3}-[0-9]{3}-[0-9]{4}$"))
        {
            await _dialogs.ShowErrorAsync("Teléfono inválido (###-###-####).");
            return;
        }
        var c = _bank.CreateCustomer(Name, Email, Phone, GovId);
        StatusMessage = $"Cliente creado Id {c.Id}";
    }

    [RelayCommand]
    private async Task CreateAccountAsync()
    {
        var acc = AccountFactory.CreateAccount(CustomerIdForAccount, AccountType, AccountCurrency);
        if (acc == null)
        {
            await _dialogs.ShowErrorAsync("Tipo de cuenta inválido.");
            return;
        }
        _bank.Accounts.Add(acc);
        StatusMessage = $"Cuenta {acc.Number} creada";
    }

    [RelayCommand]
    private async Task SetStatusAsync()
    {
        if (string.IsNullOrWhiteSpace(AccountNumberStatus))
        {
            await _dialogs.ShowErrorAsync("Número de cuenta requerido.");
            return;
        }
        var ok = _bank.SetAccountStatus(AccountNumberStatus, NewStatus);
        StatusMessage = ok ? "Estado actualizado" : "No se pudo actualizar";
        if (!ok) await _dialogs.ShowErrorAsync("No se pudo actualizar el estado.");
    }
}
