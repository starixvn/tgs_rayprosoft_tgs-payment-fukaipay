using Tgs.Payment.Clients;
using Tgs.Payment.FuKaiPay.Client;

namespace Tgs.Payment.FuKaiPay;
public record DepositForm(
    Guid Id,
    string PlayerCode,
    Guid BrandId,
    Guid BrandPaymentId,
    decimal Amount,
    PaymentFlowState Status,
    FuKaiPayDepositApplyInfo? ApplyInfo,
    string? PaymentReturn = null);