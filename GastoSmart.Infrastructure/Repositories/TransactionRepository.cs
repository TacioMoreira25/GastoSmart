using GastoSmart.Application;
using GastoSmart.Domain.Entities;
using GastoSmart.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GastoSmart.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AddAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await Task.CompletedTask; // Emulate async for Add if needed, or leave to SaveChanges
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _context.Entry(transaction).State = EntityState.Modified;
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Transaction transaction)
    {
        _context.Transactions.Remove(transaction);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Transactions.AnyAsync(e => e.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
