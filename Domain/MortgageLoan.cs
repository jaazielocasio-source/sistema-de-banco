namespace BankSystem.Domain;
public class MortgageLoan : LoanBase
{ public MortgageLoan(int customerId, decimal principal, int termMonths, string currency): base(customerId, principal, termMonths, currency){ AnnualRate = RateCatalog.MortgageAPR; } }
