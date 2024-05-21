using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using Tgs.Payment.FuKaiPay.ApplicationServices;
using Tgs.Payment.FuKaiPay.Client;
using Tgs.Payment.FuKaiPay.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.FuKaiPay.Controllers;

[Route("[controller]")]
[ApiController]
public class DepositController(ILogger<DepositController> logger) : ControllerBase
{
    [Consumes("application/json")]
    [HttpPost("callback")]
    public async Task<string> DepositCallbackAsync(
       [FromServices] IGetService<GetDepositFormQuery, DepositForm?> getService,
       [FromServices] IPaymentServiceClient paymentServiceClient,
       [FromServices] IPaymentClient paymentClient,
       [FromServices] FuKaiPayHasher hasher,
       [FromBody] CallbackDepositViewModel source)
    {
        logger.LogInformation("{source}", source);

        var formId = Guid.Parse(source.MerchantOrderNo);

        var depositForm = await getService
            .GetAsync(new(formId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (depositForm == null)
        {
            logger.LogWarning("Can't found deposit form({id}).", formId);
            return "fail";
        }

        var merchantKey = depositForm.ApplyInfo!.Settings.MerchantKey;

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(source))!;
        _ = dict.Remove("Sign");

        var sign = source.Sign;
        var hash = hasher.ComputeHash(
            merchantKey,
            dict.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value.ToString())));

        logger.LogInformation("Compute hash: {hash}, sign: {sign}", hash, sign);

        if (sign != hash)
        {
            logger.LogWarning("Sign value invalid. input: {sign}, compute: {hash}", sign, hash);
            return "fail";
        }

        var paymentReturn = new FuKaiPayDepositReturn()
        {
            OrderAmount = decimal.Parse(source.PaymentAmount),
            FormId = formId,
            Status = source.Status.ToString(),
            Sign = source.Sign,
            MerchantAccount = source.MerchantAccount,
            ThirdPartyOrderNo = source.OrderNo,
            PaymentTime = source.PaymentTime,
            RealAmount = decimal.Parse(source.Amount),
            ChannelCode = source.ChannelCode,
            Remark = source.Remark,
        };

        var fsm = PaymentStateMachineFactory.CreateDepositStateMachine(depositForm.Status);

        switch (paymentReturn.Status)
        {
            case "1"://成功
                if (fsm.CanFire(PaymentTrigger.PaymentDepositCompleted))
                {
                    await fsm.FireAsync(PaymentTrigger.PaymentDepositCompleted).ConfigureAwait(false);

                    await paymentServiceClient.SubmitDepositFormStatusAsync(
                        depositForm.Id,
                        paymentReturn: JsonSerializer.Serialize(paymentReturn, FuKaiPayUtility.SerializerOptions),
                        cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

                    await paymentClient.ApproveDepositAsync(
                        depositForm.Id,
                        depositForm.BrandId,
                        paymentReturn.OrderAmount,
                        HttpContext.RequestAborted).ConfigureAwait(false);
                }

                break;

            default:
                break;
        }

        return "success";
    }
}