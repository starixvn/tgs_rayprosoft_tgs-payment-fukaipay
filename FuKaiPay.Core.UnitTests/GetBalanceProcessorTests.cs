using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.FuKaiPay;
using Tgs.Payment.FuKaiPay.MessageQueue;
using Tgs.Payment.Protos;

namespace FuKaiPay.Core.UnitTests;

public class GetBalanceProcessorTests
{
    [Fact]
    public async Task GetBalanceProcessor_取得商戶餘額()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");

        var fakeTimeProvider = Substitute.For<TimeProvider>();

        var fakePayClient = new FuKaiPayClient(
            httpClient,
            new FuKaiPayHasher(NullLogger<FuKaiPayHasher>.Instance),
            fakeTimeProvider,
            NullLogger<FuKaiPayClient>.Instance);

        var sut = new GetBalanceProcessor(fakePayClient);

        var request = new PaymentBalanceRequest
        {
            BrandPaymentSettings = """
                {
                    "merchantId": "merchantId",
                    "merchantKey": "merchantKey"
                }
                """
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/SendTransaction/getBalance")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 200,
                    "msg": "success",
                    "data": {
                        "MerchantAccount": "merchantId",
                        "Balance": "0.00",
                        "Blockedbalance": "0.00",
                        "SumBalance": "0.00"
                    }
                }
                """));

        // Act
        var actual = await sut.HandleAsync("test", request, null);

        // Assert
        Assert.Equal(0M, actual.Balance.ToDecimal());
    }
}