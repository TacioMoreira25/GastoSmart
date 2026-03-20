namespace GastoSmart.Domain.Entities;

public class User : BaseEntity
{
    public string SupabaseId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal AlertThresholdPercentage { get; set; } = 20m;

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
