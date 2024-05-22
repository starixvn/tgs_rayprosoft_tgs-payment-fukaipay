using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Tgs.Payment.FuKaiPay;

namespace FuKaiPay.Core.UnitTests;

public class FuKaiPayClientTests
{
    [Fact]
    public async Task 代收_MAYA()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new FuKaiPayHasher(NullLogger<FuKaiPayHasher>.Instance);

        var sut = new FuKaiPayClient(httpClient, signer, TimeProvider.System, NullLogger<FuKaiPayClient>.Instance);

        var formId = Guid.Parse("b667ff5c-b142-4460-9a52-bab2bf4617e4");
        var merchantId = "merchantId";
        var merchantKey = "merchantKey";

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

        var order = new DepositOrder(
           500m,
           formId,
           new Uri("https://dev-payment.rpgp.cc/fukaipay/deposit/callback"),
           "MAYA",
           "4ucmqplxb029",
           "PHP",
           "127.0.0.1");

        var actual = await sut.DepositAsync(merchantId, merchantKey, order);

        Assert.Equal("b667ff5c-b142-4460-9a52-bab2bf4617e4", actual.ThirdPartyOrderNo);
        Assert.Equal(new Uri("https://fukui.exlepoa.com/api/payment_direct/redirect/SUPN4528948909460180992"), actual.RedirectUrl);
    }

    [Fact]
    public async Task 代收_GCASH()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new FuKaiPayHasher(NullLogger<FuKaiPayHasher>.Instance);

        var sut = new FuKaiPayClient(httpClient, signer, TimeProvider.System, NullLogger<FuKaiPayClient>.Instance);

        var formId = Guid.Parse("2e042362-41dd-4fad-95bb-e6cee2216251");
        var merchantId = "merchantId";
        var merchantKey = "merchantKey";

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/SendTransaction/unifiedorder")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 200,
                    "msg": "success",
                    "data": {
                        "qrcode": "https:\/\/fukui.exlepoa.com\/api\/payment_direct\/redirect\/HDRB4529214901780238336"
                    }
                }
                """));

        var order = new DepositOrder(
           800m,
           formId,
           new Uri("https://bc9lx48d-7014.asse.devtunnels.ms/deposit/callback"),
           "GCASH",
           "4ucmqplxb029",
           "PHP",
           "127.0.0.1");

        var actual = await sut.DepositAsync(merchantId, merchantKey, order);

        Assert.Equal("2e042362-41dd-4fad-95bb-e6cee2216251", actual.ThirdPartyOrderNo);
        Assert.Equal(new Uri("https://fukui.exlepoa.com/api/payment_direct/redirect/HDRB4529214901780238336"), actual.RedirectUrl);
    }

    [Fact]
    public async Task 代收訂單查詢()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new FuKaiPayHasher(NullLogger<FuKaiPayHasher>.Instance);

        var sut = new FuKaiPayClient(httpClient, signer, TimeProvider.System, NullLogger<FuKaiPayClient>.Instance);

        var formId = Guid.Parse("db7c8883cc0a462d97139b0ee20f3de6");
        var merchantId = "merchantId";
        var merchantKey = "merchantKey";

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/SendTransaction/queryTransaction")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 200,
                    "msg": "success",
                    "data": {
                        "MerchantAccount": "merchantId",
                        "MerchantOrderNo": "b667ff5c-b142-4460-9a52-bab2bf4617e4",
                        "OrderNo": "SUPN4528948909460180992",
                        "PaymentAmount": "500.00",
                        "PaymentCurrency": "PHP",
                        "PaymentStatus": 0,
                        "PaymentTime": "",
                        "Sign": "RiCRZPHBIQ3HowK9Mgws0qPPtnO+OS+ZyDb4XFhwfL0="
                    }
                }
                """));

        var actual = await sut.QueryDepositFormAsync(merchantId, merchantKey, formId);

        Assert.Equal(0, actual);
    }

    [Fact]
    public async Task 餘額查詢()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new FuKaiPayHasher(NullLogger<FuKaiPayHasher>.Instance);

        var sut = new FuKaiPayClient(httpClient, signer, TimeProvider.System, NullLogger<FuKaiPayClient>.Instance);

        var merchantId = "merchantId";
        var merchantKey = "merchantKey";

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

        var actual = await sut.QueryBalanceAsync(merchantId, merchantKey);

        Assert.Equal(0, actual);
    }

    [Fact]
    public async Task 代付_MAYA()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new FuKaiPayHasher(NullLogger<FuKaiPayHasher>.Instance);

        var sut = new FuKaiPayClient(httpClient, signer, TimeProvider.System, NullLogger<FuKaiPayClient>.Instance);

        var formId = Guid.Parse("94182b09-5375-4d21-86ce-9e06492e3cda");
        var merchantId = "merchantId";
        var merchantKey = "merchantKey";

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/ReceiveCurrencyList/unifiedorder")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 200,
                    "msg": "Withdrawal application successful",
                    "MerchantOrderNo": "94182b09-5375-4d21-86ce-9e06492e3cda"
                }
                """));

        var order = new WithdrawOrder(
           formId,
           100m,
           new Uri("https://dev-payment.rpgp.cc/fukaipay/withdraw/callback"),
           "MAYA",
           "4ucmqplxb029",
           "112233445566",
           "PMP",
           "PHP",
           "127.0.0.1");

        var actual = await sut.WithdrawAsync(merchantId, merchantKey, order);

        Assert.Equal(100m, actual.Amount);
        Assert.Equal(formId, actual.FormId);
        Assert.Equal("94182b09-5375-4d21-86ce-9e06492e3cda", actual.ThirdPartyOrderNo);
    }
}