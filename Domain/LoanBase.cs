namespace BankSystem.Domain;
public abstract class LoanBase : IReportable, IPayable
{
    protected LoanBase(int customerId, decimal principal, int termMonths, string currency)
    { LoanId = IdGenerator.NewLoanId(); CustomerId=customerId; Principal=principal; TermMonths=termMonths; Currency=currency; }
    public string LoanId { get; }
    public int CustomerId { get; }
    public decimal Principal { get; protected set; }
    public int TermMonths { get; protected set; }
    public string Currency { get; protected set; }
    public decimal AnnualRate { get; protected set; }
    public LoanStatus Status { get; protected set; } = LoanStatus.Current;
    public int MissedPayments { get; protected set; }
    public DateTime? LastPaymentDate { get; protected set; }
    public virtual decimal CalculateInstallment()
    {
        var monthlyRate = AnnualRate/12m; if(monthlyRate==0) return Principal/TermMonths;
        double r=(double)monthlyRate; int n=TermMonths; double p=(double)Principal;
        return (decimal)( p * (r * Math.Pow(1+r,n)) / (Math.Pow(1+r,n)-1) );
    }
    public record AmortRow(decimal Payment, decimal InterestPortion, decimal PrincipalPortion, decimal RemainingBalance);
    public virtual List<AmortRow> GenerateAmortizationTable()
    {
        var list=new List<AmortRow>(); var monthlyRate=AnnualRate/12m; var balance=Principal; var payment=CalculateInstallment();
        for(int i=0;i<TermMonths;i++){ var interest=balance*monthlyRate; var principalPortion=payment-interest; balance-=principalPortion; if(i==TermMonths-1&&balance!=0){ payment+=balance; principalPortion+=balance; balance=0;} list.Add(new AmortRow(payment,interest,principalPortion,balance)); }
        return list;
    }
    public bool ProcessPayment(decimal amount, DateTime date)
    {
        Principal = Math.Max(0, Principal-amount);
        LastPaymentDate = date;
        Status = Principal == 0 ? LoanStatus.PaidOff : LoanStatus.Current;
        MissedPayments = 0;
        return true;
    }
    public void RegisterMissedPayment()
    {
        if (Status == LoanStatus.PaidOff) return;
        MissedPayments++;
        Status = MissedPayments >=3 ? LoanStatus.Defaulted : LoanStatus.Delinquent;
    }
    public string ToCsv()=> $"{LoanId},{CustomerId},{Currency},{Principal:F2},{AnnualRate:P},{TermMonths}";
}
