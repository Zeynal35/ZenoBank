namespace ZenoBank.Services.Transaction.Application.DTOs;

public class MonthlyTransactionSummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public decimal TotalDeposited { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public decimal TotalTransferred { get; set; }
    public decimal NetFlow => TotalDeposited - TotalWithdrawn;
}

public class TransactionTypeBreakdownDto
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
}

public class DashboardAnalyticsDto
{
    public List<MonthlyTransactionSummaryDto> MonthlySummary { get; set; } = new();
    public List<TransactionTypeBreakdownDto> TypeBreakdown { get; set; } = new();
    public int TotalTransactionCount { get; set; }
    public decimal TotalVolumeAllTime { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public decimal CurrentMonthDeposited { get; set; }
    public decimal CurrentMonthWithdrawn { get; set; }
}
