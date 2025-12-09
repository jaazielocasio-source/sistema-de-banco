namespace BankSystem.Services;
public static class Masking
{
    public static string MaskId(string id)=> string.IsNullOrWhiteSpace(id)||id.Length<4 ? "****" : new string('*', Math.Max(0,id.Length-4)) + id[^4..];
    public static string MaskAccount(string number)=> number.Length<=4? number : new string('•', number.Length-4)+number[^4..];
}
