namespace BankSystem.Domain;
public class PaymentSchedule
{
    public Guid Id { get; } = Guid.NewGuid();
    public string SourceAccount { get; set; } = string.Empty;
    public string DestinationId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Periodicity Periodicity { get; set; } = Periodicity.Monthly;
    public DateTime NextDate { get; set; } = DateTime.Today.AddMonths(1);
    public string Name { get; set; } = "AutoPay";
    public string? Memo { get; set; }
    public int? DayOfMonth { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public bool Active { get; set; } = true;
    public int MaxRetries { get; set; }
    public int RetryEveryDays { get; set; } = 1;
    public int RetriesDone { get; set; }
    public string LastResult { get; set; } = "Pending";
    public DateTime? LastAttempt { get; set; }
}
