using BankSystem.Domain;
namespace BankSystem.Services;
public static class AccountFactory
{
    public static AccountBase? CreateAccount(int customerId, string tipo, string currency)
    {
        return tipo switch
        {
            "1" => new SavingsAccount(customerId, currency){ InterestStrategy = new SavingsInterestStrategy() },
            "2" => new CheckingAccount(customerId, currency){ InterestStrategy = new CheckingInterestStrategy() },
            "3" => new CertificateOfDeposit(customerId, currency){ InterestStrategy = new CdInterestStrategy() },
            _ => null
        };
    }
}
public static class LoanFactory
{
    public static LoanBase? CreateLoan(int customerId, string tipo, decimal principal, int termMonths, string currency)
        => tipo switch
        {
            "1" => new PersonalLoan(customerId, principal, termMonths, currency),
            "2" => new MortgageLoan(customerId, principal, termMonths, currency),
            "3" => new AutoLoan(customerId, principal, termMonths, currency),
            _ => null
        };
}
