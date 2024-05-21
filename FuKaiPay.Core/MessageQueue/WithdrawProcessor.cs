using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.Clients;
using Tgs.Payment.FuKaiPay.Client;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.FuKaiPay.MessageQueue;

internal class WithdrawProcessor(
    FuKaiPayClient fuKaiPayClient,
    IOptions<FuKaiPayOptions> optionsAccessor,
    IPaymentClient paymentClient)
    : IMessageProcessor<WithdrawRequest, WithdrawResponse>
{
    private readonly FuKaiPayOptions _options = optionsAccessor.Value;

    public async ValueTask<WithdrawResponse> HandleAsync(
        string subject,
        WithdrawRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        var settings = JsonSerializer.Deserialize<FuKaiPayWithdrawSettings>(data.ApplyInfo, FuKaiPayUtility.SerializerOptions)!;

        var order = new WithdrawOrder(
            settings.FormId,
            decimal.Parse(data.Amount),
            new Uri($"{_options.CallbackBaseUrl}/withdraw/callback"),
            settings.ChannelCode,
            settings.PayerName,
            settings.PayerAccount,
            settings.PayerBankName,
            settings.PaymentCurrency,
            settings.MemberIP);

        var withdrawResponse = await fuKaiPayClient
            .WithdrawAsync(settings.MerchantId, settings.MerchantKey, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new FuKaiPayWithdrawApplyInfo(
            settings,
            new FuKaiPayWithdrawResponse(withdrawResponse.FormId, withdrawResponse.ThirdPartyOrderNo));

        await paymentClient
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                settings.FormId,
                withdrawResponse.ThirdPartyOrderNo,
                cancellationToken)
            .ConfigureAwait(false);

        return new WithdrawResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, FuKaiPayUtility.SerializerOptions)
        };
    }
}