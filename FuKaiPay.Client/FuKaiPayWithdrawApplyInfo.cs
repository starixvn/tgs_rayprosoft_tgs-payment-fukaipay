namespace Tgs.Payment.FuKaiPay.Client;
public record FuKaiPayWithdrawApplyInfo(
    FuKaiPayWithdrawSettings Settings,
    FuKaiPayWithdrawResponse Response);