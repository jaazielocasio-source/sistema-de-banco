using BankSystem.Domain;
namespace BankSystem.Services;
public class SavingsInterestStrategy : IInterestStrategy
{
    public void Apply(AccountBase account, DateTime date, bool monthly)
    {
        var rate = monthly? RateCatalog.SavingsMonthlyRate : RateCatalog.SavingsDailyRate;
        var interest = account.Balance*rate;
        if(interest>0){
            var field=typeof(AccountBase).GetField("_balance", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            var current=(decimal)field!.GetValue(account)!; field.SetValue(account,current+interest);
            account.Transactions.Add(new Transaction{Type=TransactionType.Interest,Amount=interest,Description= monthly? "Interés mensual":"Interés diario"});
            AuditLogger.Log("INTEREST.APPLY", $"Intereses aplicados a {account.Number} ({account.GetType().Name}) {interest:F4}");
        }
    }
}
public class CheckingInterestStrategy : IInterestStrategy
{
    public void Apply(AccountBase account, DateTime date, bool monthly)
    {
        var field=typeof(AccountBase).GetField("_balance", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
        var balance=(decimal)field!.GetValue(account)!;
        if(balance<0){
            var rate = monthly? RateCatalog.CheckingOverdraftMonthlyRate : RateCatalog.CheckingOverdraftDailyRate;
            var fee = Math.Abs(balance)*rate; field.SetValue(account, balance-fee);
            account.Transactions.Add(new Transaction{Type=TransactionType.Fee,Amount=fee,Description="Cargo sobregiro"});
            AuditLogger.Log("OVERDRAFT.FEE", $"Cargo sobregiro a {account.Number} {fee:F4}");
        }
    }
}
public class CdInterestStrategy : IInterestStrategy
{
    public void Apply(AccountBase account, DateTime date, bool monthly)
    {
        var rate = monthly? RateCatalog.CdMonthlyRate : RateCatalog.CdDailyRate;
        var field=typeof(AccountBase).GetField("_balance", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
        var balance=(decimal)field!.GetValue(account)!; var interest=balance*rate;
        if(interest>0){ field.SetValue(account,balance+interest); account.Transactions.Add(new Transaction{Type=TransactionType.Interest,Amount=interest,Description= monthly? "Interés CD mensual":"Interés CD diario"}); AuditLogger.Log("INTEREST.CD", $"Interés CD a {account.Number} {interest:F4}"); }
    }
}
