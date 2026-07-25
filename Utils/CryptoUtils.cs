using System.Security.Cryptography;
using System.Text;
using System;

namespace GastoSmart.Utils;

public static class CryptoUtils
{
    public static string HashPin(string pin)
    {
        if (string.IsNullOrEmpty(pin))
            return string.Empty;

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(pin);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
