using Tgs.Payment.Clients;
using Tgs.Payment.FuKaiPay.Client;

namespace Tgs.Payment.FuKaiPay;
public record WithdrawForm(
      Guid Id,
      string? UserId,
      UserType UserType,
      Guid BrandId,
      Guid? BrandPaymentId,
      decimal Amount,
      PaymentFlowState Status,
      Guid BankInfoId,
      string? BankInfo,
      FuKaiPayWithdrawApplyInfo? ApplyInfo,
      string? PaymentReturn = null);