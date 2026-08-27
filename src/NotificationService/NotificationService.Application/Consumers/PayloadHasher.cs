using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NotificationService.Application.Consumers;

internal static class PayloadHasher
{
    public static string ComputeHash<T>(T payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
