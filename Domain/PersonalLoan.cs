namespace BankSystem.Domain;
public class PersonalLoan : LoanBase
{ public PersonalLoan(int customerId, decimal principal, int termMonths, string currency): base(customerId, principal, termMonths, currency){ AnnualRate = RateCatalog.PersonalLoanAPR; } }
