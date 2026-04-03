namespace GastoSmart.Application.DTOs;

public class TransactionRequestDTO
{
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? ReceiptUrl { get; set; }
    public bool IsAiGenerated { get; set; }
    public Guid CategoryId { get; set; }
    public Guid UserId { get; set; }
}
