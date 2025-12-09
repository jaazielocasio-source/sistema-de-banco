using BankSystem.Services;
using BankSystem.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace BankSystem.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private ObservableObject _currentView;

    public RelayCommand NavigateDashboardCommand { get; }
    public RelayCommand NavigateAccountsCommand { get; }
    public RelayCommand NavigateLoansCommand { get; }
    public RelayCommand NavigateTransferCommand { get; }
    public RelayCommand NavigateAutoPayCommand { get; }
    public RelayCommand NavigateReportsCommand { get; }
    public RelayCommand NavigateAuditCommand { get; }
    public RelayCommand NavigateAdminCommand { get; }

    private readonly DashboardViewModel _dashboard;
    private readonly AccountsViewModel _accounts;
    private readonly LoansViewModel _loans;
    private readonly AutoPayViewModel _autopay;
    private readonly ReportsViewModel _reports;
    private readonly AuditLogViewModel _audit;
    private readonly AdminViewModel _admin;
    private readonly DialogService _dialogs;
    private readonly BankService _bank;

    public MainWindowViewModel(
        DashboardViewModel dashboard,
        AccountsViewModel accounts,
        LoansViewModel loans,
        AutoPayViewModel autopay,
        ReportsViewModel reports,
        AuditLogViewModel audit,
        AdminViewModel admin,
        DialogService dialogs,
        BankService bank)
    {
        _dashboard = dashboard;
        _accounts = accounts;
        _loans = loans;
        _autopay = autopay;
        _reports = reports;
        _audit = audit;
        _admin = admin;
        _dialogs = dialogs;
        _bank = bank;
        _currentView = dashboard;

        NavigateDashboardCommand = new RelayCommand(() => CurrentView = _dashboard);
        NavigateAccountsCommand = new RelayCommand(() => CurrentView = _accounts);
        NavigateLoansCommand = new RelayCommand(() => CurrentView = _loans);
        NavigateTransferCommand = new RelayCommand(async () => await OpenTransferDialogAsync());
        NavigateAutoPayCommand = new RelayCommand(() => CurrentView = _autopay);
        NavigateReportsCommand = new RelayCommand(() => CurrentView = _reports);
        NavigateAuditCommand = new RelayCommand(() => CurrentView = _audit);
        NavigateAdminCommand = new RelayCommand(() => CurrentView = _admin);
    }

    private async Task OpenTransferDialogAsync()
    {
        var vm = new TransferDialogViewModel(_bank, _dialogs);
        var result = await _dialogs.ShowTransferDialogAsync(vm);
        if (result)
        {
            _dashboard.Refresh();
            _accounts.Refresh();
        }
    }
}
