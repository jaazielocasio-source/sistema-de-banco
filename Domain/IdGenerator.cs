namespace BankSystem.Domain;
public static class IdGenerator
{
    private static int _accSeq = 100000;
    private static int _custSeq = 1;
    public static string NewAccountNumber()=> $"AC{_accSeq++}";
    public static string NewLoanId()=> $"LN{Guid.NewGuid().ToString()[..8].ToUpper()}";
    public static int NewCustomerId()=> _custSeq++;
}
