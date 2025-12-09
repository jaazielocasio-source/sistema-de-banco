namespace BankSystem.Domain;
public interface IPayable { bool ProcessPayment(decimal amount, DateTime date); }
public interface IReportable { string ToCsv(); }
public interface IInterestStrategy { void Apply(AccountBase account, DateTime date, bool monthly); }
