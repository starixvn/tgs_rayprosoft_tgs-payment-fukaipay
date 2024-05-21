using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.FuKaiPay;

public class FuKaiPayHasher(ILogger<FuKaiPayHasher> logger)
{
    public string ComputeHash(string merchantKey, IEnumerable<KeyValuePair<string, string?>> dict)
    {
        var content = string.Join(
            "&",
            dict.Where(kvp => string.IsNullOrWhiteSpace(kvp.Value) is false)
                .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                .Select(p => $"{p.Key}={p.Value}"));
        

        logger.LogTrace("FullText: {content}", content);

        var data = HMACSHA256.HashData(Encoding.UTF8.GetBytes(merchantKey), Encoding.UTF8.GetBytes(content));

        return Convert.ToBase64String(data);
    }
}