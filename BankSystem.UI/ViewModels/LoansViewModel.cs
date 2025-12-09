using BankSystem.Domain;
using BankSystem.Services;
using BankSystem.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BankSystem.UI.ViewModels;

/// <summary>
/// ViewModel para gestión de préstamos
/// Permite solicitar préstamos y ver tabla de amortización
/// </summary>
public partial class LoansViewModel : ObservableObject
{
    // Servicios de negocio
    private readonly BankService _bank;
    private readonly DialogService _dialogs;

    // Colección de préstamos activos
    public ObservableCollection<LoanBase> Loans { get; } = new();
    
    // Tabla de amortización del préstamo seleccionado
    public ObservableCollection<LoanBase.AmortRow> Amortization { get; } = new();

    // Préstamo seleccionado para ver detalles
    [ObservableProperty] private LoanBase? _selectedLoan;
    
    // Campos para solicitar nuevo préstamo
    [ObservableProperty] private int _customerId;
    [ObservableProperty] private string _loanType = "1";  // 1=Personal, 2=Hipotecario, 3=Auto
    [ObservableProperty] private decimal _principal = 1000;  // Monto del préstamo
    [ObservableProperty] private int _months = 12;  // Plazo en meses
    [ObservableProperty] private string _currency = "USD";
    
    // Cuota mensual calculada
    [ObservableProperty] private decimal _installment;

    public LoansViewModel(BankService bank, DialogService dialogs)
    {
        _bank = bank;
        _dialogs = dialogs;
        Refresh();
    }

    public void Refresh()
    {
        Loans.Clear();
        foreach (var l in _bank.Loans) Loans.Add(l);
        if (SelectedLoan != null)
        {
            SelectedLoan = Loans.FirstOrDefault(l => l.LoanId == SelectedLoan.LoanId);
        }
    }

    partial void OnSelectedLoanChanged(LoanBase? value)
    {
        Amortization.Clear();
        if (value == null) return;
        foreach (var row in value.GenerateAmortizationTable()) Amortization.Add(row);
        Installment = value.CalculateInstallment();
    }

    [RelayCommand]
    private async Task CreateLoanAsync()
    {
        try
        {
            if (CustomerId <= 0 || Principal <= 0 || Months <= 0)
            {
                await _dialogs.ShowErrorAsync("❌ Datos inválidos para préstamo.\n\nVerifica que:\n• ID del Cliente sea mayor a 0\n• Monto Principal sea mayor a 0\n• Plazo en meses sea mayor a 0");
                return;
            }
            
            var loan = _bank.CreateLoan(CustomerId, LoanType, Principal, Months, Currency);
            if (loan == null)
            {
                await _dialogs.ShowErrorAsync("❌ Tipo de préstamo inválido.\n\nSelecciona un tipo válido: Personal, Hipotecario o Auto.");
                return;
            }
            
            // Agregar a ambas listas para mantener sincronización
            _bank.Loans.Add(loan);
            Loans.Add(loan);
            SelectedLoan = loan;
            
            // Mostrar mensaje de éxito
            var loanTypeName = LoanType == "1" ? "Personal" : LoanType == "2" ? "Hipotecario" : "Auto";
            await _dialogs.ShowMessageAsync(
                "✅ Préstamo Creado Exitosamente", 
                $"Se ha creado el préstamo {loanTypeName}:\n\n" +
                $"💰 Monto: {Principal:C2}\n" +
                $"📅 Plazo: {Months} meses\n" +
                $"💳 Cuota mensual: {loan.CalculateInstallment():C2}\n\n" +
                $"El préstamo está visible en la tabla de abajo."
            );
        }
        catch (System.Exception ex)
        {
            await _dialogs.ShowErrorAsync($"❌ Error al crear préstamo:\n\n{ex.Message}");
        }
    }
}
