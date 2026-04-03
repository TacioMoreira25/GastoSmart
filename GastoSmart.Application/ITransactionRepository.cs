using GastoSmart.Domain.Entities;

namespace GastoSmart.Application;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetAllAsync();
    Task<Transaction?> GetByIdAsync(Guid id);
    Task AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task DeleteAsync(Transaction transaction);
    Task<bool> ExistsAsync(Guid id);
    Task SaveChangesAsync();
}
