using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.Payment.FuKaiPay.Client;
using Xunit.Abstractions;

namespace Tgs.Payment.FuKaiPay.IntegrationTests;

public class DepositTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    [Fact]
    public async Task 代收回調_訂單狀態成功()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = FuKaiPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("2e042362-41dd-4fad-95bb-e6cee2216251");
        var brandId = Guid.NewGuid();
        var merchantKey = "merchantKey";

        var request = """
            {
                "Status": 1,
                "MerchantAccount": "merchantId",
                "ChannelCode": "GCASH",
                "SuccessUrl": "",
                "MerchantOrderNo": "2e042362-41dd-4fad-95bb-e6cee2216251",
                "OrderNo": "HDRB4529214901780238336",
                "PaymentAmount": "800.00",
                "Amount": "800.00",
                "ExtendFields": "",
                "PaymentTime": "2024-05-21 09:53:49",
                "Remark": "",
                "Sign": "ws7KIo/ihNhDRpje6p/Qa9JQ9xZ9blY+tuSHYOMWS/Q="
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var applyInfo = new FuKaiPayDepositApplyInfo(
            new FuKaiPayDepositSettings(
                "merchantId",
                merchantKey,
                formId,
                "GCASH",
                "4ucmqplxb029",
                "PHP",
                "127.0.0.1"),
             new FuKaiPayDepositResponse(
                800m,
                formId,
                new Uri("https://fukui.exlepoa.com/api/payment_direct/redirect/HDRB4529214901780238336"),
                "2e042362-41dd-4fad-95bb-e6cee2216251"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId), Arg.Any<CancellationToken>())
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.WaitPayment,
                Amount = 800m,
                ThirdPartyOrderNo = "2e042362-41dd-4fad-95bb-e6cee2216251",
                ApplyInfo = JsonSerializer.Serialize(applyInfo, FuKaiPayUtility.SerializerOptions)
            });

        var response = await httpClient.PostAsync("/deposit/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("success", actual);

        var paymentReturn = new FuKaiPayDepositReturn()
        {
            Status = "1",
            FormId = formId,
            OrderAmount = decimal.Parse("800.00"),
            Sign = "ws7KIo/ihNhDRpje6p/Qa9JQ9xZ9blY+tuSHYOMWS/Q=",
            MerchantAccount = "merchantId",
            ThirdPartyOrderNo = "HDRB4529214901780238336",
            PaymentTime = "2024-05-21 09:53:49",
            RealAmount = decimal.Parse("800.00"),
            ChannelCode = "GCASH",
            Remark = ""
        };

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .ApproveDepositAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Is(paymentReturn.OrderAmount),
                Arg.Any<CancellationToken>());
    }
}