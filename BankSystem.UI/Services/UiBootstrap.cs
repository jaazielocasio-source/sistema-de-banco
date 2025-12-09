using BankSystem.Domain;
using BankSystem.Services;
using BankSystem.UI.ViewModels;

namespace BankSystem.UI.Services;

public class UiBootstrap
{
    public BankService Bank { get; }
    public SchedulerService Scheduler { get; }
    public DialogService Dialogs { get; }
    public MainWindowViewModel MainWindowViewModel { get; }

    public UiBootstrap()
    {
        AuditLogger.Init("./logs/audit.log");
        Bank = new BankService();
        Scheduler = new SchedulerService();
        Dialogs = new DialogService();
        Seed();

        var dashboard = new DashboardViewModel(Bank, Scheduler, Dialogs);
        var accounts = new AccountsViewModel(Bank, Dialogs);
        var loans = new LoansViewModel(Bank, Dialogs);
        var autopay = new AutoPayViewModel(Bank, Scheduler, Dialogs);
        var reports = new ReportsViewModel(Bank, Dialogs);
        var audit = new AuditLogViewModel(Dialogs);
        var admin = new AdminViewModel(Bank, Dialogs);

        MainWindowViewModel = new MainWindowViewModel(
            dashboard,
            accounts,
            loans,
            autopay,
            reports,
            audit,
            admin,
            Dialogs,
            Bank);
    }

    private void Seed()
    {
        if (Bank.Customers.Any()) return;
        var c1 = Bank.CreateCustomer("Ana López", "ana@example.com", "787-000-0000", "123-45-6789");
        var a1 = AccountFactory.CreateAccount(c1.Id, "1", "USD")!; Bank.Accounts.Add(a1); Bank.Deposit(a1.Number, 1200);
        var c2 = Bank.CreateCustomer("Luis Pérez", "luis@example.com", "787-111-1111", "987-65-4321");
        var a2 = AccountFactory.CreateAccount(c2.Id, "2", "USD")!; Bank.Accounts.Add(a2); Bank.Deposit(a2.Number, 300);
        Bank.Transfer(a1.Number, a2.Number, 100);
        Bank.SchedulePayment(a1.Number, a2.Number, 50, Periodicity.Monthly, 15, DateTime.Today.AddDays(1), null, 1, 2, "Renta", "Pago de renta");
    }
}
