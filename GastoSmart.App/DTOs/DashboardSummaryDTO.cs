namespace Gastosmart.App.DTOs;

public class DashboardSummaryDTO
{
    public decimal TotalBalance { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public List<CategorySummaryDTO> CategorySummaries { get; set; } = new();
    public List<TransactionResponseDTO> RecentTransactions { get; set; } = new();
}
