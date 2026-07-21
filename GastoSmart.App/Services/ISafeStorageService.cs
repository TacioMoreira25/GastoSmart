namespace GastoSmart.App.Services;

public interface ISafeStorageService
{
    Task SaveTokenAsync(string key, string value);
    Task<string?> GetTokenAsync(string key);
    void RemoveToken(string key);
}