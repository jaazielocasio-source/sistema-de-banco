namespace BankSystem.Domain;
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money operator +(Money a, Money b)
    {
        if (!string.Equals(a.Currency, b.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("No se puede sumar dinero con distintas monedas.");
        return new Money(a.Amount + b.Amount, a.Currency);
    }
}
