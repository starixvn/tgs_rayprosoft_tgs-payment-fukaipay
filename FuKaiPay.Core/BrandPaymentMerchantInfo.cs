using System.Text.Json.Serialization;

namespace Tgs.Payment.FuKaiPay;
public record BrandPaymentMerchantInfo(
    [property: JsonPropertyName("merchantId")] string MerchantId,
    [property: JsonPropertyName("merchantKey")] string MerchantKey);