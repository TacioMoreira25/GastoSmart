using GastoSmart.Application;
using GastoSmart.Application.DTOs;
using GastoSmart.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GastoSmart.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionRepository _repository;

    public TransactionsController(ITransactionRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionResponseDTO>>> GetTransactions()
    {
        var transactions = await _repository.GetAllAsync();
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

    [HttpPost]
    public async Task<ActionResult<TransactionResponseDTO>> CreateTransaction(TransactionRequestDTO request)
    {
        var transaction = new Transaction
        {
            Title = request.Title,
            Amount = request.Amount,
            Date = request.Date,
            ReceiptUrl = request.ReceiptUrl,
            IsAiGenerated = request.IsAiGenerated,
            CategoryId = request.CategoryId,
            UserId = request.UserId
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
}
