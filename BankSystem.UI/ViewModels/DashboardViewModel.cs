using BankSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        Refresh();
    }

    [RelayCommand]
    public void Refresh()
    {
        TotalBalance = _bank.Accounts.Sum(a => a.Balance);
        AccountCount = _bank.Accounts.Count;
        UpcomingPayments.Clear();
        foreach (var p in _bank.ScheduledPayments.OrderBy(p => p.NextDate).Take(5))
        {
            UpcomingPayments.Add($"{p.NextDate:yyyy-MM-dd}: {p.SourceAccount}->{p.DestinationId} {p.Amount} ({p.Periodicity})");
        }
    }
}
