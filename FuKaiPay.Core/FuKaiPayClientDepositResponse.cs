namespace Tgs.Payment.FuKaiPay;
public record FuKaiPayClientDepositResponse(
    decimal Amount,
    Guid FormId,
    string ThirdPartyOrderNo,
    Uri RedirectUrl);