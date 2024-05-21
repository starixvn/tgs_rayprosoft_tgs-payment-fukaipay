namespace Tgs.Payment.FuKaiPay.Client;

public record FuKaiPayWithdrawSettings(
    string MerchantId,
    string MerchantKey,
    Guid FormId,
    string ChannelCode,
    string PayerName,
    string PayerAccount,
    string PayerBankName,
    string PaymentCurrency,
    string MemberIP);