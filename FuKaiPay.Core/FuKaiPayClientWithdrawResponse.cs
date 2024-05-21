namespace Tgs.Payment.FuKaiPay;
public record FuKaiPayClientWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo,
    decimal Amount);