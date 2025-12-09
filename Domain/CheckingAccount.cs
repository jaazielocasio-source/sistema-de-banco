namespace BankSystem.Domain;
public class CheckingAccount : AccountBase
{
    public CheckingAccount(int customerId, string currency) : base(customerId, currency) { }
    public decimal OverdraftLimit { get; set; } = 200m;
    protected override bool HasSufficientFunds(decimal amount) => Balance - amount >= -OverdraftLimit;
}
