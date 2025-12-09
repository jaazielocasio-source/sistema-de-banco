using BankSystem.Domain;
using BankSystem.Services;
using BankSystem.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BankSystem.UI.ViewModels;

public partial class LoansViewModel : ObservableObject
{
    private readonly BankService _bank;
    private readonly DialogService _dialogs;

    public ObservableCollection<LoanBase> Loans { get; } = new();
    public ObservableCollection<LoanBase.AmortRow> Amortization { get; } = new();

    [ObservableProperty] private LoanBase? _selectedLoan;
    [ObservableProperty] private int _customerId;
    [ObservableProperty] private string _loanType = "1";
    [ObservableProperty] private decimal _principal = 1000;
    [ObservableProperty] private int _months = 12;
    [ObservableProperty] private string _currency = "USD";
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
        if (CustomerId <= 0 || Principal <= 0 || Months <= 0)
        {
            await _dialogs.ShowErrorAsync("Datos inválidos para préstamo.");
            return;
        }
        var loan = _bank.CreateLoan(CustomerId, LoanType, Principal, Months, Currency);
        if (loan == null)
        {
            await _dialogs.ShowErrorAsync("Tipo de préstamo inválido.");
            return;
        }
        _bank.Loans.Add(loan);
        Refresh();
        SelectedLoan = loan;
    }
}
