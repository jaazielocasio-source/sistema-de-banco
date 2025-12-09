using BankSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BankSystem.UI.ViewModels;

public partial class TransferDialogViewModel : ObservableObject
{
    private readonly BankService _bank;
    private readonly Services.DialogService _dialogs;

    public ObservableCollection<string> Accounts { get; } = new();

    [ObservableProperty] private string? _sourceAccount;
    [ObservableProperty] private string? _destinationAccount;
    [ObservableProperty] private decimal _amount = 100;
    [ObservableProperty] private string? _estimate;
    [ObservableProperty] private string? _status;

    public Action<bool>? CloseAction { get; set; }

    public TransferDialogViewModel(BankService bank, Services.DialogService dialogs)
    {
        _bank = bank;
        _dialogs = dialogs;
        foreach (var acc in bank.Accounts) Accounts.Add(acc.Number);
    }

    partial void OnSourceAccountChanged(string? value)
    {
        UpdateEstimate();
        ConfirmCommand.NotifyCanExecuteChanged();
    }
    partial void OnDestinationAccountChanged(string? value)
    {
        UpdateEstimate();
        ConfirmCommand.NotifyCanExecuteChanged();
    }
    partial void OnAmountChanged(decimal value)
    {
        UpdateEstimate();
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    private void UpdateEstimate()
    {
        var src = _bank.Accounts.FirstOrDefault(a => a.Number == SourceAccount);
        var dst = _bank.Accounts.FirstOrDefault(a => a.Number == DestinationAccount);
        if (src == null || dst == null) { Estimate = null; return; }
        if (src.Currency.Equals(dst.Currency, StringComparison.OrdinalIgnoreCase))
        {
            Estimate = $"{Amount:F2} {dst.Currency}";
        }
        else
        {
            var converted = CurrencyConverter.Convert(Amount, src.Currency, dst.Currency);
            Estimate = converted.HasValue ? $"{converted.Value:F2} {dst.Currency} (convertido)" : "Moneda no soportada";
        }
    }

    private bool CanConfirm() => Amount > 0 && !string.IsNullOrWhiteSpace(SourceAccount) && !string.IsNullOrWhiteSpace(DestinationAccount);

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task ConfirmAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SourceAccount) || string.IsNullOrWhiteSpace(DestinationAccount)) 
            {
                await _dialogs.ShowErrorAsync("Por favor selecciona las cuentas de origen y destino.");
                return;
            }
            
            if (SourceAccount == DestinationAccount)
            {
                await _dialogs.ShowErrorAsync("No puedes transferir a la misma cuenta.");
                return;
            }
            
            var ok = _bank.Transfer(SourceAccount, DestinationAccount, Amount);
            if (!ok)
            {
                Status = "❌ Falló la transferencia";
                await _dialogs.ShowErrorAsync("Transferencia inválida o fondos insuficientes.");
                return;
            }
            
            // Transferencia exitosa - mostrar mensaje y limpiar el formulario
            var transferAmount = Amount;
            Status = $"✅ Transferencia de {transferAmount:C2} completada exitosamente. Puedes hacer otra transferencia.";
            
            // Limpiar el formulario para permitir una nueva transferencia
            Amount = 100;
            SourceAccount = null;
            DestinationAccount = null;
            Estimate = null;
            
            // Actualizar las cuentas disponibles
            Accounts.Clear();
            foreach (var acc in _bank.Accounts) Accounts.Add(acc.Number);
            
            // Limpiar el mensaje de estado después de 5 segundos
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000);
                Status = null;
            });
            
            // El diálogo permanece abierto para nuevas transferencias
            // NO cerrar: CloseAction?.Invoke(true);
        }
        catch (Exception ex)
        {
            Status = $"❌ Error: {ex.Message}";
            await _dialogs.ShowErrorAsync($"Error al realizar la transferencia: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Clear()
    {
        // Limpiar formulario
        Amount = 100;
        SourceAccount = null;
        DestinationAccount = null;
        Estimate = null;
        Status = "📝 Formulario limpiado. Listo para nueva transferencia.";
        
        // Limpiar mensaje después de 3 segundos
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);
            Status = null;
        });
    }

    [RelayCommand]
    private void Cancel() => CloseAction?.Invoke(false);
}
