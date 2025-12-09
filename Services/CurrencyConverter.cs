namespace BankSystem.Services;
public static class CurrencyConverter
{
    private static readonly Dictionary<string, decimal> _usdBase = new(StringComparer.OrdinalIgnoreCase)
    { ["USD"]=1m, ["EUR"]=0.92m, ["JPY"]=151m, ["KRW"]=1380m, ["BOB"]=6.9m, ["GBP"]=0.78m };
    public static decimal? Convert(decimal amount, string from, string to)
    {
        if(!_usdBase.ContainsKey(from) || !_usdBase.ContainsKey(to)) return null;
        var inUsd = amount/_usdBase[from]; return inUsd*_usdBase[to];
    }
}
