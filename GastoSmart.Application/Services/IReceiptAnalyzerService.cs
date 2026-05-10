using GastoSmart.Application.DTOs;

namespace GastoSmart.Application.Services;

public interface IReceiptAnalyzerService
{
    Task<TransactionRequestDTO> AnalyzeReceiptAsync(Stream imageStream);
}
