using BankSystem.Domain;
namespace BankSystem.Services;
public class BankService
{
    private const decimal MaxWithdrawal = 5000m;
    private const decimal MaxTransfer = 20000m;

    public List<Customer> Customers { get; } = new();
    public List<AccountBase> Accounts { get; } = new();
    public List<LoanBase> Loans { get; } = new();
    public List<PaymentSchedule> ScheduledPayments { get; } = new();

    public Customer CreateCustomer(string name, string email, string phone, string govId)
    { 
        var c=new Customer{Id=IdGenerator.NewCustomerId(),Name=name,Email=email,Phone=phone,GovernmentId=govId};
        Customers.Add(c);
        AuditLogger.Log("CUSTOMER.CREATE",$"Cliente {c.Id} creado", govId);
        return c;
    }

    public IEnumerable<AccountBase> GetAccountsByCustomer(int id)=> Accounts.Where(a=>a.CustomerId==id);
    public IEnumerable<LoanBase> GetLoansByCustomer(int id)=> Loans.Where(l=>l.CustomerId==id);

    public bool SetAccountStatus(string number, AccountStatus status)
    { 
        var a=Accounts.FirstOrDefault(x=>x.Number==number); if(a==null) return false; 
        a.SetStatus(status); 
        AuditLogger.Log("ACCOUNT.STATUS",$"Cuenta {number} -> {status}", number);
        return true; 
    }
    public bool FreezeAccount(string number)=> SetAccountStatus(number, AccountStatus.Frozen);
    public bool UnfreezeAccount(string number)=> SetAccountStatus(number, AccountStatus.Active);

    public bool Deposit(string number, decimal amount)
    { 
        var a=Accounts.FirstOrDefault(x=>x.Number==number); if(a==null) return false; 
        var ok=a.Deposit(amount); 
        AuditLogger.Log("TX.DEPOSIT",$"Cuenta {number} Depósito {amount}", number);
        return ok; 
    }

    public bool Withdraw(string number, decimal amount)
    { 
        if (amount<=0 || amount>MaxWithdrawal) { AuditLogger.Log("TX.WITHDRAW.LIMIT",$"Monto fuera de límite ({amount}) max={MaxWithdrawal}", number); return false; }
        var a=Accounts.FirstOrDefault(x=>x.Number==number); if(a==null) return false; 
        var ok=a.Withdraw(amount); 
        AuditLogger.Log("TX.WITHDRAW",$"Cuenta {number} Retiro {amount}", number);
        return ok; 
    }

    public bool Transfer(string from, string to, decimal amount)
    {
        if (amount<=0 || amount>MaxTransfer) { AuditLogger.Log("TX.TRANSFER.LIMIT",$"Monto fuera de límite ({amount}) max={MaxTransfer}", from); return false; }
        var a=Accounts.FirstOrDefault(x=>x.Number==from); var b=Accounts.FirstOrDefault(x=>x.Number==to);
        if(a==null||b==null||amount<=0) return false; if(a.Status!=AccountStatus.Active||b.Status==AccountStatus.Closed) return false;
        bool ok;
        if(!a.Currency.Equals(b.Currency,StringComparison.OrdinalIgnoreCase)){ var converted=CurrencyConverter.Convert(amount,a.Currency,b.Currency); if(converted==null) return false; if(!a.Withdraw(amount)) return false; b.Deposit(converted.Value); ok=true; }
        else { if(!a.Withdraw(amount)) return false; b.Deposit(amount); ok=true; }
        AuditLogger.Log("TX.TRANSFER",$"{from} -> {to} {amount}", from);
        return ok;
    }

    public void ApplyInterestToAll(DateTime date, bool monthly){ foreach(var a in Accounts) a.ApplyInterest(date, monthly); AuditLogger.Log("INTEREST.BATCH", $"Aplicación de intereses a {Accounts.Count} cuentas (monthly={monthly})"); }

    public LoanBase? CreateLoan(int customerId, string tipo, decimal principal, int termMonths, string currency)
    { var loan=LoanFactory.CreateLoan(customerId,tipo,principal,termMonths,currency); if(loan!=null) AuditLogger.Log("LOAN.CREATE.SVC",$"Loan {loan.LoanId} {currency} {principal} {termMonths}m para {customerId}"); return loan; }

    public bool SchedulePayment(string sourceAccount, string destId, decimal amount, string periodicity)
    {
        var src=Accounts.FirstOrDefault(a=>a.Number==sourceAccount); if(src==null||amount<=0) return false;
        var p=new PaymentSchedule{ SourceAccount=sourceAccount, DestinationId=destId, Amount=amount, Periodicity= periodicity.Equals("Daily",StringComparison.OrdinalIgnoreCase)? Periodicity.Daily:Periodicity.Monthly, NextDate=DateTime.Today.AddDays(1)};
        ScheduledPayments.Add(p);
        AuditLogger.Log("SCHEDULE.ADD",$"{sourceAccount} -> {destId} {amount} {p.Periodicity} next={p.NextDate:yyyy-MM-dd}", sourceAccount);
        return true;
    }

    public bool SchedulePayment(string sourceAccount, string destId, decimal amount, Periodicity periodicity, int? dayOfMonth, DateTime startDate, DateTime? endDate, int maxRetries, int retryEveryDays, string? name = null, string? memo=null)
    {
        var src = Accounts.FirstOrDefault(a => a.Number == sourceAccount); if (src == null || amount <= 0) return false;
        var next = startDate.Date;
        if (dayOfMonth.HasValue && periodicity == Periodicity.Monthly)
        {
            var days = DateTime.DaysInMonth(next.Year, next.Month);
            var day = Math.Min(dayOfMonth.Value, days);
            next = new DateTime(next.Year, next.Month, day);
            if (next < DateTime.Today) next = next.AddMonths(1);
        }
        var p = new PaymentSchedule
        {
            SourceAccount = sourceAccount,
            DestinationId = destId,
            Amount = amount,
            Periodicity = periodicity,
            NextDate = next,
            DayOfMonth = dayOfMonth,
            StartDate = startDate.Date,
            EndDate = endDate?.Date,
            MaxRetries = Math.Max(0, maxRetries),
            RetryEveryDays = Math.Max(1, retryEveryDays),
            Name = string.IsNullOrWhiteSpace(name) ? "AutoPay" : name,
            Memo = memo
        };
        ScheduledPayments.Add(p);
        AuditLogger.Log("SCHEDULE.ADD",$"{sourceAccount} -> {destId} {amount} {p.Periodicity} next={p.NextDate:yyyy-MM-dd} retries={p.MaxRetries}", sourceAccount);
        return true;
    }
}
