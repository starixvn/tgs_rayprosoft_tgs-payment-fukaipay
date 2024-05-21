namespace Tgs.Payment.FuKaiPay.Client;
public record FuKaiPayDepositResponse(
    decimal Amount,
    Guid FormId,
    Uri RedirectUrl,
    string ThirdPartyOrderNo);