namespace Tgs.Payment.FuKaiPay.Client;
public record FuKaiPayWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo);