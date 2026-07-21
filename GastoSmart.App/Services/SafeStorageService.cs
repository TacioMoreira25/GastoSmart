using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace GastoSmart.App.Services;

public class SafeStorageService : ISafeStorageService
{
    public SafeStorageService()
    {
        if (!Preferences.Default.ContainsKey("has_run_before"))
        {
            SecureStorage.Default.RemoveAll();
            Preferences.Default.Set("has_run_before", true);
        }
    }

    public async Task SaveTokenAsync(string key, string value)
    {
        await SecureStorage.Default.SetAsync(key, value);
    }

    public async Task<string?> GetTokenAsync(string key)
    {
        return await SecureStorage.Default.GetAsync(key);
    }

    public void RemoveToken(string key)
    {
        SecureStorage.Default.Remove(key);
    }
}