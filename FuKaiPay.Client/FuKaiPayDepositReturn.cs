namespace Tgs.Payment.FuKaiPay.Client;
public record FuKaiPayDepositReturn
{
    public decimal OrderAmount { get; init; }
    public Guid FormId { get; init; }

    public string Status { get; init; } = default!;

    public string Sign { get; init; } = default!;

    public string MerchantAccount { get; set; } = default!;

    public string ThirdPartyOrderNo { get; set; } = default!;

    public string PaymentTime { get; set; } = default!;

    public decimal RealAmount { get; init; }

    public string ChannelCode { get; set; } = default!;

    public string Remark { get; set; } = default!;
}