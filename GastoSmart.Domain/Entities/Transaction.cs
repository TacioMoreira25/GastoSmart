namespace GastoSmart.Domain.Entities;

public class Transaction : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? ReceiptUrl { get; set; }
    public bool IsAiGenerated { get; set; }
    public Guid? IdempotencyKey { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
