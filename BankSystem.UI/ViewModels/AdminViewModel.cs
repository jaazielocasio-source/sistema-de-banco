using BankSystem.Domain;
using BankSystem.Services;
using BankSystem.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BankSystem.UI.ViewModels;

/// <summary>
/// ViewModel de administración para operaciones privilegiadas
/// Permite crear clientes, cuentas y cambiar estados de cuentas
/// </summary>
public partial class AdminViewModel : ObservableObject
{
    // Servicios de negocio
    private readonly BankService _bank;
    private readonly DialogService _dialogs;

    // Opciones disponibles para el estado de cuentas
    public AccountStatus[] StatusOptions { get; } = new[] { AccountStatus.Active, AccountStatus.Closed, AccountStatus.Frozen };

    // Campos para crear nuevo cliente
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;  // Formato: ###-###-#### (opcional)
    [ObservableProperty] private string _govId = string.Empty;

    // Campos para crear nueva cuenta
    [ObservableProperty] private int _customerIdForAccount;
    [ObservableProperty] private string _accountType = "1";  // 1=Ahorro, 2=Corriente, 3=CD
    [ObservableProperty] private string _accountCurrency = "USD";

    // Campos para cambiar estado de cuenta
    [ObservableProperty] private string _accountNumberStatus = string.Empty;
    [ObservableProperty] private AccountStatus _newStatus = AccountStatus.Active;

    // Mensaje de estado de las operaciones
    [ObservableProperty] private string? _statusMessage;

    /// <summary>
    /// Constructor: inicializa servicios de administración
    /// </summary>
    public AdminViewModel(BankService bank, DialogService dialogs)
    {
        _bank = bank;
        _dialogs = dialogs;
    }

    [RelayCommand]
    private async Task CreateCustomerAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await _dialogs.ShowErrorAsync("El nombre es requerido.");
                return;
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                await _dialogs.ShowErrorAsync("El email es requerido.");
                return;
            }
            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await _dialogs.ShowErrorAsync("Email inválido. Use el formato: ejemplo@correo.com");
                return;
            }
            // Validar teléfono solo si no está vacío
            if (!string.IsNullOrWhiteSpace(Phone) && !Regex.IsMatch(Phone, @"^[0-9]{3}-[0-9]{3}-[0-9]{4}$"))
            {
                await _dialogs.ShowErrorAsync("Teléfono inválido. Use el formato: ###-###-#### o déjelo vacío.");
                return;
            }
            var c = _bank.CreateCustomer(Name, Email, Phone ?? string.Empty, GovId);
            StatusMessage = $"✅ Cliente creado con ID: {c.Id}";
            
            // Limpiar campos
            Name = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            GovId = string.Empty;
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
            await _dialogs.ShowErrorAsync($"Error al crear cliente: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CreateAccountAsync()
    {
        try
        {
            if (CustomerIdForAccount <= 0)
            {
                await _dialogs.ShowErrorAsync("ID de cliente inválido.");
                return;
            }
            
            var acc = AccountFactory.CreateAccount(CustomerIdForAccount, AccountType, AccountCurrency);
            if (acc == null)
            {
                await _dialogs.ShowErrorAsync("Tipo de cuenta inválido.");
                return;
            }
            _bank.Accounts.Add(acc);
            StatusMessage = $"✅ Cuenta {acc.Number} creada exitosamente";
            
            // Resetear valores
            CustomerIdForAccount = 0;
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
            await _dialogs.ShowErrorAsync($"Error al crear cuenta: {ex.Message}");
        }
    }

    /// <summary>
    /// Comando para activar o desactivar una tarjeta (cuenta) seleccionándola visualmente
    /// </summary>
    [RelayCommand]
    private async Task SetStatusAsync()
    {
        try
        {
            // Obtener todas las cuentas disponibles
            var accounts = _bank.Accounts.ToList();
            if (!accounts.Any())
            {
                await _dialogs.ShowErrorAsync("No hay cuentas disponibles.");
                return;
            }

            // Mostrar selector visual de tarjetas
            var selectedAccount = await _dialogs.ShowAccountSelectorAsync("Selecciona la tarjeta a activar/desactivar", accounts);
            if (selectedAccount == null) return;

            // Actualizar el estado de la cuenta seleccionada
            var ok = _bank.SetAccountStatus(selectedAccount.Number, NewStatus);
            StatusMessage = ok ? $"✅ Tarjeta {selectedAccount.Number} actualizada a {NewStatus}" : "❌ No se pudo actualizar";
            if (!ok) await _dialogs.ShowErrorAsync("No se pudo actualizar el estado.");
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
            await _dialogs.ShowErrorAsync($"Error al actualizar estado: {ex.Message}");
        }
    }
}
