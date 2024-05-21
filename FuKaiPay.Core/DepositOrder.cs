namespace Tgs.Payment.FuKaiPay;
public record DepositOrder(
    decimal Amount,
    Guid FormId,
    Uri CallbackUrl,
    string ChannelCode,
    string PayerName,
    string PaymentCurrency,
    string MemberIP);