namespace Tgs.Payment.FuKaiPay.Client;
public record FuKaiPayWithdrawReturn
{
    public decimal OrderAmount { get; init; }

    public Guid FormId { get; init; }

    public string Status { get; init; } = default!;

    public string Sign { get; init; } = default!;
    public string ChannelCode { get; set; } = default!;
    public string MerchantAccount { get; set; } = default!;
    public string PaymentTime { get; set; } = default!;

    public string ThirdPartyOrderNo { get; set; } = default!;

    public decimal RealAmount { get; init; }

    public string Remark { get; set; } = default!;
}