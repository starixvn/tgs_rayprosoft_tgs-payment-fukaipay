namespace Tgs.Payment.FuKaiPay.Client;

public record FuKaiPayDepositSettings(
    string MerchantId,
    string MerchantKey,
    Guid FormId,
    string ChannelCode,
    string PayerName,
    string PaymentCurrency,
    string MemberIP);