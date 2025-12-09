namespace BankSystem.Domain;
public class SavingsAccount : AccountBase
{
    public SavingsAccount(int customerId, string currency) : base(customerId, currency) { }
    public int WithdrawalsThisMonth { get; private set; } = 0;
    protected override bool HasSufficientFunds(decimal amount) => Balance - amount >= 0m;
    public override bool Withdraw(decimal amount)
    {
        if (WithdrawalsThisMonth >= FeeSchedule.SavingsMonthlyWithdrawalLimit) return false;
        var ok = base.Withdraw(amount);
        if (ok) WithdrawalsThisMonth++;
        return ok;
    }
    public override void ApplyInterest(DateTime date, bool monthly){ base.ApplyInterest(date,monthly); if(monthly&&date.Day==1) WithdrawalsThisMonth=0; }
}
