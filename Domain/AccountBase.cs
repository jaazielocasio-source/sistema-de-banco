namespace BankSystem.Domain;
public abstract class AccountBase : IReportable, IPayable
{
    private decimal _balance;
    protected AccountBase(int customerId, string currency)
    { CustomerId = customerId; Currency = currency; Number = IdGenerator.NewAccountNumber(); Status = AccountStatus.Active; }
    public string Number { get; protected set; }
    public int CustomerId { get; protected set; }
    public string Currency { get; protected set; }
    public AccountStatus Status { get; protected set; }
    public List<Transaction> Transactions { get; } = new();
    public IInterestStrategy? InterestStrategy { get; init; }
    public decimal Balance { get => _balance; protected set => _balance = value; }
    public void SetStatus(AccountStatus status) => Status = status;
    public virtual bool Deposit(decimal amount){ if(Status!=AccountStatus.Active||amount<=0) return false; Balance+=amount; Transactions.Add(new Transaction{Type=TransactionType.Deposit,Amount=amount,Description="Depósito"}); return true; }
    public virtual bool Withdraw(decimal amount){ if(Status!=AccountStatus.Active||amount<=0) return false; if(!HasSufficientFunds(amount)) return false; Balance-=amount; Transactions.Add(new Transaction{Type=TransactionType.Withdrawal,Amount=amount,Description="Retiro"}); return true; }
    protected abstract bool HasSufficientFunds(decimal amount);
    public virtual void ApplyInterest(DateTime date, bool monthly)=> InterestStrategy?.Apply(this,date,monthly);
    public bool ProcessPayment(decimal amount, DateTime date)=> Withdraw(amount);
    public string ToCsv()=> $"{Number},{CustomerId},{Currency},{Balance:F2},{Status}";
}
