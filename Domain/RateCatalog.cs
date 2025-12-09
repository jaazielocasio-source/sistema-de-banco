namespace BankSystem.Domain;
public static class RateCatalog
{
    public const decimal SavingsDailyRate = 0.01m/365m;
    public const decimal SavingsMonthlyRate = 0.01m/12m;
    public const decimal CdDailyRate = 0.03m/365m;
    public const decimal CdMonthlyRate = 0.03m/12m;
    public const decimal CheckingOverdraftDailyRate = 0.18m/365m;
    public const decimal CheckingOverdraftMonthlyRate = 0.18m/12m;
    public const decimal PersonalLoanAPR = 0.12m;
    public const decimal MortgageAPR = 0.065m;
    public const decimal AutoLoanAPR = 0.08m;
}
