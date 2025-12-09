namespace BankSystem.Domain;
public enum AccountStatus { Active, Closed, Frozen }
public enum TransactionType { Deposit, Withdrawal, TransferOut, TransferIn, Fee, Interest }
public enum Periodicity { Daily, Monthly }
public enum LoanStatus { Current, Delinquent, Defaulted, PaidOff }
