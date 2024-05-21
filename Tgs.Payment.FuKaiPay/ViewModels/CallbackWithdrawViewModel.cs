namespace Tgs.Payment.FuKaiPay.ViewModels;

public class CallbackWithdrawViewModel
{
    public int Status { get; set; }
    public string MerchantAccount { get; set; } = default!;
    public string ChannelCode { get; set; } = default!;
    public string SuccessUrl { get; set; } = default!;
    public string MerchantOrderNo { get; set; } = default!;
    public string OrderNo { get; set; } = default!;
    public string PaymentAmount { get; set; } = default!;
    public string Amount { get; set; } = default!;
    public string ExtendFields { get; set; } = default!;
    public string PaymentTime { get; set; } = default!;
    public string Remark { get; set; } = default!;
    public string Sign { get; set; } = default!;
}