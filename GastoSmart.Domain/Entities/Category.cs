using GastoSmart.Domain.Enums;

namespace GastoSmart.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    
    // If null, it's a default system category.
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
