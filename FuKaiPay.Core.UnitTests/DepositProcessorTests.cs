using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.Clients;
using Tgs.Payment.FuKaiPay;
using Tgs.Payment.FuKaiPay.Client;
using Tgs.Payment.FuKaiPay.MessageQueue;
using Tgs.Payment.Protos;

namespace FuKaiPay.Core.UnitTests;

public class DepositProcessorTests
{
    [Fact]
    public async Task 儲值要求處理()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");

        var fakePayClient = new FuKaiPayClient(
            httpClient,
            new FuKaiPayHasher(NullLogger<FuKaiPayHasher>.Instance),
            TimeProvider.System,
            NullLogger<FuKaiPayClient>.Instance);

        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var options = new FuKaiPayOptions
        {
            CallbackBaseUrl = new Uri("https://dev-payment.rpgp.cc/fukaipay"),
            PaymentApiUrl = new Uri("https://localhost"),
            PaymentServiceBaseUrl = new Uri("http://localhost:5541")
        };
        var formId = Guid.Parse("b667ff5c-b142-4460-9a52-bab2bf4617e4");
        var merchantId = "merchantId";
        var merchantKey = "merchantKey";

        var sut = new DepositProcessor(
            fakePayClient,
            new OptionsWrapper<FuKaiPayOptions>(options),
            NullLogger<DepositProcessor>.Instance);

        var applySettings = new FuKaiPayDepositSettings(
            merchantId,
            merchantKey,
            formId,
            "MAYA",
            "4ucmqplxb029",
            "PHP",
            "127.0.0.1");

        var request = new DepositRequest
        {
            Amount = "500",
            ApplyInfo = JsonSerializer.Serialize(applySettings, FuKaiPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/SendTransaction/unifiedorder")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 200,
                    "msg": "success",
                    "data": {
                        "qrcode": "https:\/\/fukui.exlepoa.com\/api\/payment_direct\/redirect\/SUPN4528948909460180992"
                    }
                }
                """));

        // Act
        var actual = await sut.HandleAsync(
            "tgs.payment.fukaiPay.deposit",
            request,
            null);

        var applyInfo = JsonSerializer.Deserialize<FuKaiPayDepositApplyInfo>(actual.Result, FuKaiPayUtility.SerializerOptions);

        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
        Assert.Equal(
            new FuKaiPayDepositResponse(
                500m,
                applySettings.FormId,
                new Uri("https://fukui.exlepoa.com/api/payment_direct/redirect/SUPN4528948909460180992"),
                "b667ff5c-b142-4460-9a52-bab2bf4617e4"),
            applyInfo.Response);
    }
}