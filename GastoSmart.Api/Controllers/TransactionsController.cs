using System.Security.Claims;
using GastoSmart.Application;
using GastoSmart.Application.DTOs;
using GastoSmart.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GastoSmart.Application.Services;
using GastoSmart.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;

namespace GastoSmart.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionRepository _repository;
    private readonly IReceiptAnalyzerService _receiptAnalyzerService;
    private readonly AppDbContext _context;

    public TransactionsController(ITransactionRepository repository, IReceiptAnalyzerService receiptAnalyzerService, AppDbContext context)
    {
        _repository = repository;
        _receiptAnalyzerService = receiptAnalyzerService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionResponseDTO>>> GetTransactions()
    {
        var supabaseId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    
        if (string.IsNullOrEmpty(supabaseId))
        {
            return Unauthorized("Utilizador não autenticado ou token inválido.");
        }

        var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.SupabaseId == supabaseId);
    
        if (dbUser == null)
        {
            return Ok(new List<TransactionResponseDTO>());
        }

        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == dbUser.Id)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        var dtos = transactions.Select(t => new TransactionResponseDTO
        {
            Id = t.Id,
            Title = t.Title,
            Amount = t.Amount,
            Date = t.Date,
            CategoryName = t.Category?.Name ?? string.Empty
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionResponseDTO>> GetTransaction(Guid id)
    {
        var transaction = await _repository.GetByIdAsync(id);

        if (transaction == null)
        {
            return NotFound();
        }

        var dto = new TransactionResponseDTO
        {
            Id = transaction.Id,
            Title = transaction.Title,
            Amount = transaction.Amount,
            Date = transaction.Date,
            CategoryName = transaction.Category?.Name ?? string.Empty
        };

        return Ok(dto);
    }
    
    [HttpGet("dashboard-summary")]
    public async Task<ActionResult<DashboardSummaryDTO>> GetDashboardSummary()
    {
        var supabaseId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(supabaseId))
            return Unauthorized("Utilizador não autenticado.");

        var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.SupabaseId == supabaseId);
        if (dbUser == null)
            return Ok(new DashboardSummaryDTO());

        var hoje = DateTime.UtcNow;
        var inicioDoMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var transacoes = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == dbUser.Id)
            .ToListAsync();

        var resumo = new DashboardSummaryDTO();

        resumo.MonthlySpend = transacoes
            .Where(t => t.Date >= inicioDoMes)
            .Sum(t => t.Amount);

        resumo.TotalBalance = 5000m - resumo.MonthlySpend; 

        resumo.CategorySummaries = transacoes
            .Where(t => t.Date >= inicioDoMes)
            .GroupBy(t => t.Category)
            .Select(g => new CategorySummaryDTO
            {
                CategoryName = g.Key?.Name ?? "Outros",
                TotalAmount = g.Sum(t => t.Amount),
            })
            .ToList();

        resumo.RecentTransactions = transacoes
            .OrderByDescending(t => t.Date)
            .Take(5)
            .Select(t => new TransactionResponseDTO
            {
                Id = t.Id,
                Title = t.Title,
                Amount = t.Amount,
                Date = t.Date,
                CategoryName = t.Category?.Name ?? string.Empty
            })
            .ToList();

        return Ok(resumo);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionResponseDTO>> CreateTransaction(TransactionRequestDTO request,
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey)
    {
        var supabaseId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        
        if (string.IsNullOrEmpty(supabaseId))
        {
            return Unauthorized("Utilizador não autenticado ou token inválido.");
        }

        var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.SupabaseId == supabaseId);
        if (dbUser == null)
        {
            var email = User.FindFirstValue(System.Security.Claims.ClaimTypes.Email) ?? "usuario@gastosmart.com";
            
            dbUser = new User 
            { 
                Id = Guid.NewGuid(), 
                SupabaseId = supabaseId, 
                Name = email.Split('@')[0],
                Email = email 
            };
            _context.Users.Add(dbUser);
            await _context.SaveChangesAsync();
        }

        request.UserId = dbUser.Id;

        if (request.CategoryId == Guid.Empty)
        {
            var defaultCategory = await _context.Categories.FirstOrDefaultAsync(c => c.UserId == dbUser.Id);
            if (defaultCategory == null)
            {
                defaultCategory = new Category { Id = Guid.NewGuid(), Name = "Outros", UserId = dbUser.Id };
                _context.Categories.Add(defaultCategory);
                await _context.SaveChangesAsync();
            }
            request.CategoryId = defaultCategory.Id;
        }

        var existingTransaction = await _repository.GetByIdempotencyKeyAsync(idempotencyKey);
        if (existingTransaction != null)
        {
            var existingDto = new TransactionResponseDTO
            {
                Id = existingTransaction.Id,
                Title = existingTransaction.Title,
                Amount = existingTransaction.Amount,
                Date = existingTransaction.Date,
                CategoryName = existingTransaction.Category?.Name ?? string.Empty
            };
            return Ok(existingDto);
        }

        var transaction = new Transaction
        {
            Title = request.Title,
            Amount = request.Amount,
            Date = request.Date,
            ReceiptUrl = request.ReceiptUrl,
            IsAiGenerated = request.IsAiGenerated,
            CategoryId = request.CategoryId,
            UserId = request.UserId, 
            IdempotencyKey = idempotencyKey
        };

        await _repository.AddAsync(transaction);
        await _repository.SaveChangesAsync();

        var savedTransaction = await _repository.GetByIdAsync(transaction.Id);

        var dto = new TransactionResponseDTO
        {
            Id = savedTransaction!.Id,
            Title = savedTransaction.Title,
            Amount = savedTransaction.Amount,
            Date = savedTransaction.Date,
            CategoryName = savedTransaction.Category?.Name ?? string.Empty
        };

        return CreatedAtAction(nameof(GetTransaction), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(Guid id, TransactionRequestDTO request)
    {
        var transaction = await _repository.GetByIdAsync(id);
        if (transaction == null)
        {
            return NotFound();
        }

        transaction.Title = request.Title;
        transaction.Amount = request.Amount;
        transaction.Date = request.Date;
        transaction.ReceiptUrl = request.ReceiptUrl;
        transaction.IsAiGenerated = request.IsAiGenerated;
        transaction.CategoryId = request.CategoryId;
        transaction.UserId = request.UserId;

        await _repository.UpdateAsync(transaction);

        try
        {
            await _repository.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await TransactionExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        var transaction = await _repository.GetByIdAsync(id);
        if (transaction == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(transaction);
        await _repository.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> TransactionExists(Guid id)
    {
        return await _repository.ExistsAsync(id);
    }

    [HttpPost("scan-receipt")]
    public async Task<ActionResult<TransactionRequestDTO>> ScanReceipt(IFormFile receiptImage)
    {
        if (receiptImage == null || receiptImage.Length == 0)
        {
            return BadRequest("A valid receipt image is required.");
        }

        using var stream = receiptImage.OpenReadStream();
        var transactionDto = await _receiptAnalyzerService.AnalyzeReceiptAsync(stream);

        return Ok(transactionDto);
    }
}
