namespace Tgs.Payment.FuKaiPay;
public record WithdrawOrder(
    Guid FormId,
    decimal Amount,
    Uri CallbackUrl,
    string ChannelCode,
    string PayerName,
    string PayerAccount,
    string PayerBankName,
    string PaymentCurrency,
    string MemberIP);