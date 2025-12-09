using BankSystem.Domain;
using BankSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BankSystem.UI.ViewModels;

public partial class AccountDetailViewModel : ObservableObject
{
    private readonly BankService _bank;

    public object?[] FilterOptions { get; } = new object?[] { null, TransactionType.Deposit, TransactionType.Withdrawal, TransactionType.TransferOut, TransactionType.TransferIn, TransactionType.Fee, TransactionType.Interest };

    [ObservableProperty] private string _accountNumber = string.Empty;
    [ObservableProperty] private string _accountType = string.Empty;
    [ObservableProperty] private string _currency = string.Empty;
    [ObservableProperty] private decimal _balance;
    [ObservableProperty] private AccountStatus _status;
    [ObservableProperty] private DateTime? _fromDate;
    [ObservableProperty] private DateTime? _toDate;
    [ObservableProperty] private TransactionType? _filterType;
    [ObservableProperty] private int _transactionCount;

    public ObservableCollection<Transaction> Transactions { get; } = new();

    public AccountDetailViewModel(BankService bank)
    {
        _bank = bank;
    }

    public void Load(string accountNumber)
    {
        var acc = _bank.Accounts.FirstOrDefault(a => a.Number == accountNumber);
        if (acc == null) return;
        AccountNumber = acc.Number;
        AccountType = acc.GetType().Name;
        Currency = acc.Currency;
        Balance = acc.Balance;
        Status = acc.Status;
        RefreshTransactions();
    }

    [RelayCommand]
    private void RefreshTransactions()
    {
        Transactions.Clear();
        var acc = _bank.Accounts.FirstOrDefault(a => a.Number == AccountNumber);
        if (acc == null) return;
        var query = acc.Transactions.AsEnumerable();
        if (FromDate.HasValue) query = query.Where(t => t.Date >= FromDate.Value);
        if (ToDate.HasValue) query = query.Where(t => t.Date <= ToDate.Value);
        if (FilterType.HasValue) query = query.Where(t => t.Type == FilterType.Value);
        foreach (var t in query.OrderByDescending(t => t.Date)) Transactions.Add(t);
        TransactionCount = Transactions.Count;
    }
}
