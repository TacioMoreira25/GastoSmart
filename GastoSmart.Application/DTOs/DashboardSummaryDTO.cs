namespace GastoSmart.Application.DTOs;

public class DashboardSummaryDTO
{
    public decimal TotalBalance { get; set; } 
    public decimal MonthlySpend { get; set; } 
    public List<CategorySummaryDTO> CategorySummaries { get; set; } = new();
    public List<TransactionResponseDTO> RecentTransactions { get; set; } = new();
}