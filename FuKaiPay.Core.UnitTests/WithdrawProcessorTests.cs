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

public class WithdrawProcessorTests
{
    [Fact]
    public async Task 代付()
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
            CallbackBaseUrl = new Uri("https://dev-payment.rpgp.cc/fukaiPay"),
            PaymentApiUrl = new Uri("https://localhost"),
            PaymentServiceBaseUrl = new Uri("http://localhost:5541")
        };

        var sut = new WithdrawProcessor(
            fakePayClient,
            new OptionsWrapper<FuKaiPayOptions>(options),
            fakePaymentClient);

        var formId = Guid.Parse("94182b09-5375-4d21-86ce-9e06492e3cda");
        var merchantId = "merchantId";
        var merchantKey = "merchantKey";

        var applySettings = new FuKaiPayWithdrawSettings(
            merchantId,
            merchantKey,
            formId,
            "MAYA",
            "4ucmqplxb029",
            "112233445566",
            "PMP",
            "PHP",
            "127.0.0.1");

        var request = new WithdrawRequest
        {
            Amount = "100",
            ApplyInfo = JsonSerializer.Serialize(applySettings, FuKaiPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/ReceiveCurrencyList/unifiedorder")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 200,
                    "msg": "Withdrawal application successful",
                    "MerchantOrderNo": "94182b09-5375-4d21-86ce-9e06492e3cda"
                }
                """));

        // Act
        var actual = await sut.HandleAsync(
            "tgs.payment.fukaiPay.withdraw",
            request,
            null);

        var applyInfo = JsonSerializer.Deserialize<FuKaiPayWithdrawApplyInfo>(actual.Result, FuKaiPayUtility.SerializerOptions);

        // Assert
        _ = fakePaymentClient.Received(1)
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                Arg.Is(applySettings.FormId),
                Arg.Is("94182b09-5375-4d21-86ce-9e06492e3cda"));
        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
    }
}