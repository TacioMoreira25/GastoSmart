using System.Threading.Tasks;

namespace GastoSmart.Services;

public interface IGroqVisionService
{
    Task<string> AnalyzeReceiptAsync(string base64Image);
}
