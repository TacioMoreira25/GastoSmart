namespace GastoSmart.Application.DTOs;

public class TransactionResponseDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
