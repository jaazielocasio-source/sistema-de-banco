namespace BankSystem.Domain;
public class CertificateOfDeposit : AccountBase
{
    public CertificateOfDeposit(int customerId, string currency, int termMonths = 12) : base(customerId, currency){ TermMonths=termMonths; OpenDate=DateTime.Today; }
    public int TermMonths { get; private set; }
    public DateTime OpenDate { get; private set; }
    protected override bool HasSufficientFunds(decimal amount) => Balance - amount >= 0m;
    public override bool Withdraw(decimal amount)
    {
        if((DateTime.Today-OpenDate).TotalDays < TermMonths*30){
            var penalty = amount*FeeSchedule.CdEarlyWithdrawalPenaltyRate;
            var total = amount+penalty;
            if(!HasSufficientFunds(total)) return false;
            Balance -= total;
            Transactions.Add(new Transaction{Type=TransactionType.Withdrawal,Amount=amount,Description="Retiro CD (anticipado)"});
            Transactions.Add(new Transaction{Type=TransactionType.Fee,Amount=penalty,Description="Penalidad retiro CD"});
            return true;
        }
        return base.Withdraw(amount);
    }
}
