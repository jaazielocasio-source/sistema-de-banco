namespace BankSystem.Domain;
public class AutoLoan : LoanBase
{ public AutoLoan(int customerId, decimal principal, int termMonths, string currency): base(customerId, principal, termMonths, currency){ AnnualRate = RateCatalog.AutoLoanAPR; } }
