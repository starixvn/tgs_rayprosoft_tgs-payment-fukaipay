using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.FuKaiPay;

public class FuKaiPayClient(
    HttpClient httpClient,
    FuKaiPayHasher hasher,
    TimeProvider timeProvider,
    ILogger<FuKaiPayClient> logger)
{
    public async ValueTask<FuKaiPayClientDepositResponse> DepositAsync(
        string merchantId,
        string merchantKey,
        DepositOrder order,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["MerchantAccount"] = merchantId,
            ["MerchantOrderNo"] = order.FormId.ToString(),
            ["ChannelCode"] = order.ChannelCode,
            ["PayerName"] = order.PayerName,
            ["PaymentAmount"] = (order.Amount * 100m).ToString(),//單位為分，100分 = 1披索
            ["PaymentCurrency"] = order.PaymentCurrency,
            ["NotifyUrl"] = order.CallbackUrl.AbsoluteUri,
            ["TimeStart"] = timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString(),
            ["MemberIP"] = order.MemberIP
        };

        var sign = hasher.ComputeHash(merchantKey, dict);

        dict["Sign"] = sign;

        var response = await httpClient
            .PostAsJsonAsync("/SendTransaction/unifiedorder", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("FuKaiPayClient Deposit response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Deposit response can't read.");

        if (node["code"]!.GetValue<int>() != 200)
            throw new ApiFailException(node["msg"]!.ToString());

        var data = node["data"]!;

        return new FuKaiPayClientDepositResponse(
            Amount: order.Amount,
            FormId: order.FormId,
            ThirdPartyOrderNo: order.FormId.ToString(),
            RedirectUrl: new Uri(data["qrcode"]!.GetValue<string>()));
    }

    public async ValueTask<FuKaiPayClientWithdrawResponse> WithdrawAsync(
        string merchantId,
        string merchantKey,
        WithdrawOrder order,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["MerchantAccount"] = merchantId,
            ["MerchantOrderNo"] = order.FormId.ToString(),
            ["ChannelCode"] = order.ChannelCode,
            ["PayerName"] = order.PayerName,
            ["PayerAccount"] = order.PayerAccount,
            ["PaymentAmount"] = (order.Amount * 100m).ToString(),//單位為分，100分 = 1披索
            ["PayerBankName"] = order.PayerBankName,
            ["PaymentCurrency"] = order.PaymentCurrency,
            ["NotifyUrl"] = order.CallbackUrl.AbsoluteUri,
            ["TimeStart"] = timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString(),
            ["MemberIP"] = order.MemberIP
        };

        var sign = hasher.ComputeHash(merchantKey, dict);

        dict["Sign"] = sign;

        var response = await httpClient
            .PostAsJsonAsync("/ReceiveCurrencyList/unifiedorder", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("FuKaiPayClient Withdraw response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Withdraw response can't read.");

        if (node["code"]!.GetValue<int>() != 200)
            throw new ApiFailException(node["msg"]!.ToString());

        return new FuKaiPayClientWithdrawResponse(
            FormId: order.FormId,
            ThirdPartyOrderNo: node["MerchantOrderNo"]!.GetValue<string>(),
            Amount: order.Amount);
    }

    public async ValueTask<int> QueryDepositFormAsync(
        string merchantId,
        string merchantKey,
        Guid formId,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["MerchantAccount"] = merchantId,
            ["MerchantOrderNo"] = formId.ToString(),
        };

        var sign = hasher.ComputeHash(merchantKey, dict);

        dict["Sign"] = sign;

        var response = await httpClient
            .PostAsJsonAsync($"/SendTransaction/queryTransaction", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("FuKaiPayClient QueryDepositForm response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Deposit response can't read.");

        if (node["code"]!.GetValue<int>() != 200)
            throw new ApiFailException();

        var data = node["data"]!;

        return data["PaymentStatus"]!.GetValue<int>();//订单状态：0未支付，1成功，-1超时
    }

    public async ValueTask<decimal> QueryBalanceAsync(
        string merchantId,
        string merchantKey,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["MerchantAccount"] = merchantId,
        };

        var sign = hasher.ComputeHash(merchantKey, dict);

        dict["Sign"] = sign;

        var response = await httpClient
            .PostAsJsonAsync($"/SendTransaction/getBalance", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("FuKaiPayClient QueryBalance response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Balance response can't read.");

        if (node["code"]!.GetValue<int>() != 200)
            throw new ApiFailException();

        var data = node["data"]!;

        return decimal.Parse(data["SumBalance"]!.GetValue<string>());
    }
}