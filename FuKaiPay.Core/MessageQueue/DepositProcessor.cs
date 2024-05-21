using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Payment.FuKaiPay.Client;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.FuKaiPay.MessageQueue;

internal class DepositProcessor(
    FuKaiPayClient fuKaiPayClient,
    IOptions<FuKaiPayOptions> optionsAccessor,
    ILogger<DepositProcessor> logger)
    : IMessageProcessor<DepositRequest, DepositResponse>
{
    private readonly FuKaiPayOptions _options = optionsAccessor.Value;

    public async ValueTask<DepositResponse> HandleAsync(
        string subject,
        DepositRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("{DepositApplyInfo}", data.ApplyInfo);

        var settings = JsonSerializer.Deserialize<FuKaiPayDepositSettings>(
            data.ApplyInfo,
            FuKaiPayUtility.SerializerOptions)!;

        var order = new DepositOrder(
            decimal.Parse(data.Amount),
            settings.FormId,
            new Uri($"{_options.CallbackBaseUrl}/deposit/callback"),
            settings.ChannelCode,
            settings.PayerName,
            settings.PaymentCurrency,
            settings.MemberIP);

        var depositResponse = await fuKaiPayClient
            .DepositAsync(settings.MerchantId, settings.MerchantKey, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new FuKaiPayDepositApplyInfo(
            settings,
            new FuKaiPayDepositResponse(
                depositResponse.Amount,
                depositResponse.FormId,
                depositResponse.RedirectUrl,
                depositResponse.ThirdPartyOrderNo));

        return new DepositResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, FuKaiPayUtility.SerializerOptions)
        };
    }
}