using BankSystem.Domain;
using System.Collections.Generic;
namespace BankSystem.Services;
public class SchedulerService
{
    private static readonly HashSet<(int Month, int Day)> Holidays = new()
    {
        (1,1),   // Año Nuevo
        (12,25)  // Navidad
    };

    private static bool IsBusinessDay(DateTime date)
        => date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday && !Holidays.Contains((date.Month, date.Day));

    private static DateTime NextBusinessDay(DateTime date)
    {
        var d = date;
        while (!IsBusinessDay(d)) d = d.AddDays(1);
        return d;
    }

    public int ExecuteDuePayments(BankService bank, DateTime date)
    {
        int count=0;
        foreach(var p in bank.ScheduledPayments)
        {
            if (!p.Active) continue;
            if (p.EndDate.HasValue && date.Date > p.EndDate.Value.Date) { p.Active=false; p.LastResult="Finalizado"; continue; }
            var adjusted = NextBusinessDay(p.NextDate);
            if (adjusted != p.NextDate)
            {
                AuditLogger.Log("SCHEDULE.ADJUST", $"Ajustado a día hábil {p.NextDate:yyyy-MM-dd}->{adjusted:yyyy-MM-dd}", p.SourceAccount);
                p.NextDate = adjusted;
            }
            if(p.NextDate.Date<=date.Date){
                var destAcc = bank.Accounts.FirstOrDefault(a=>a.Number==p.DestinationId);
                var destLoan = bank.Loans.FirstOrDefault(l=>l.LoanId==p.DestinationId);
                var src = bank.Accounts.FirstOrDefault(a=>a.Number==p.SourceAccount);
                if(src==null) continue;
                if(!src.Withdraw(p.Amount)) {
                    p.LastAttempt = date;
                    p.LastResult = "Fondos insuficientes";
                    p.RetriesDone++;
                    AuditLogger.Log("SCHEDULE.SKIP","Fondos insuficientes", p.SourceAccount);
                    if (p.RetriesDone <= p.MaxRetries)
                    {
                        p.NextDate = NextBusinessDay(date.AddDays(p.RetryEveryDays));
                        AuditLogger.Log("SCHEDULE.RETRY",$"Retry #{p.RetriesDone} {p.SourceAccount}->{p.DestinationId} next={p.NextDate:yyyy-MM-dd}", p.SourceAccount);
                    }
                    else
                    {
                        destLoan?.RegisterMissedPayment();
                    }
                    continue;
                }
                if(destAcc!=null) destAcc.Deposit(p.Amount);
                else if(destLoan!=null) destLoan.ProcessPayment(p.Amount, date);
                else { AuditLogger.Log("SCHEDULE.SKIP","Destino no encontrado", p.DestinationId); p.LastResult="Destino no encontrado"; continue; }
                p.LastAttempt = date;
                p.LastResult = "OK";
                p.RetriesDone = 0;
                p.NextDate = p.Periodicity==Periodicity.Daily ? p.NextDate.AddDays(1) : p.NextDate.AddMonths(1);
                if (p.DayOfMonth.HasValue && p.Periodicity==Periodicity.Monthly)
                {
                    var days = DateTime.DaysInMonth(p.NextDate.Year, p.NextDate.Month);
                    var day = Math.Min(p.DayOfMonth.Value, days);
                    p.NextDate = new DateTime(p.NextDate.Year, p.NextDate.Month, day);
                }
                p.NextDate = NextBusinessDay(p.NextDate);
                AuditLogger.Log("SCHEDULE.EXEC",$"{p.SourceAccount}->{p.DestinationId} {p.Amount} next={p.NextDate:yyyy-MM-dd}", p.SourceAccount);
                count++;
            }
        }
        return count;
    }
}
