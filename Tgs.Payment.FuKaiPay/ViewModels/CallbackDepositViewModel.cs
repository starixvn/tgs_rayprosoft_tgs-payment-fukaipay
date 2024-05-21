namespace Tgs.Payment.FuKaiPay.ViewModels;

public class CallbackDepositViewModel
{
    /// <summary>
    /// 订单状态：0未支付，1成功，-1超时
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 商户ID
    /// </summary>
    public string MerchantAccount { get; set; } = default!;

    /// <summary>
    /// 渠道代码
    /// </summary>
    public string ChannelCode { get; set; } = default!;

    /// <summary>
    /// 成功跳转URL
    /// </summary>
    public string SuccessUrl { get; set; } = default!;

    /// <summary>
    /// 商户订单号
    /// </summary>
    public string MerchantOrderNo { get; set; } = default!;

    /// <summary>
    /// 系统订单号
    /// </summary>
    public string OrderNo { get; set; } = default!;

    /// <summary>
    /// 支付金额
    /// </summary>
    public string PaymentAmount { get; set; } = default!;

    /// <summary>
    /// 实际支付金额
    /// </summary>
    public string Amount { get; set; } = default!;

    /// <summary>
    /// 扩展字段
    /// </summary>
    public string ExtendFields { get; set; } = default!;

    /// <summary>
    /// 完成时间
    /// </summary>
    public string PaymentTime { get; set; } = default!;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = default!;

    /// <summary>
    /// 签名
    /// </summary>
    public string Sign { get; set; } = default!;
}