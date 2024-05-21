using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.FuKaiPay.Client;
using Tgs.Payment.Clients;
using Xunit.Abstractions;

namespace Tgs.Payment.FuKaiPay.IntegrationTests;
public class WithdrawTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    [Fact]
    public async Task 代付回調_訂單狀態失敗()
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

        var formId = Guid.Parse("6d003d7a-9c41-4510-b15f-95637c358dfe");
        var brandId = Guid.NewGuid();
        var merchantId = "merchantId";
        var merchantKey = "merchantKey";

        var settings = new FuKaiPayWithdrawSettings(
            merchantId,
            merchantKey,
            formId,
            "MAYA",
            "4ucmqplxb029",
            "112233445566",
            "PMP",
            "PHP",
            "127.0.0.1");
        var withdrawResponse = new FuKaiPayWithdrawResponse(formId, "6d003d7a-9c41-4510-b15f-95637c358dfe");

        var applyInfo = new FuKaiPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, FuKaiPayUtility.SerializerOptions)
            });

        var request = """
            {
                "Status": 2,
                "MerchantAccount": "merchantId",
                "ChannelCode": "MAYA",
                "SuccessUrl": "",
                "MerchantOrderNo": "6d003d7a-9c41-4510-b15f-95637c358dfe",
                "OrderNo": "WMRS4529280849476341760",
                "PaymentAmount": "100.00",
                "Amount": "100.00",
                "ExtendFields": "",
                "PaymentTime": "2024-05-21 13:48:38",
                "Remark": "",
                "Sign": "wpHXYm9QBqSM8lj2g1lkyFaNT7AmuZZIKlvkY3KmFxQ="
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        var paymentReturn = new FuKaiPayWithdrawReturn()
        {
            FormId = formId,
            OrderAmount = decimal.Parse("100.00"),
            Sign = "wpHXYm9QBqSM8lj2g1lkyFaNT7AmuZZIKlvkY3KmFxQ=",
            Status = "2",
            PaymentTime = "2024-05-21 13:48:38",
            MerchantAccount = "merchantId",
            ChannelCode = "MAYA",
            Remark = "",
            ThirdPartyOrderNo = "WMRS4529280849476341760",
            RealAmount = decimal.Parse("100.00"),
        };

        Assert.Equal("success", actual);
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, FuKaiPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .RejectWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 代付回調_訂單狀態成功()
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

        var formId = Guid.Parse("2cc5c6b4-1f16-4c23-a353-b3f8ae8be183");
        var brandId = Guid.NewGuid();
        var merchantId = "merchantId";
        var merchantKey = "merchantKey";

        var settings = new FuKaiPayWithdrawSettings(
            merchantId,
            merchantKey,
            formId,
            "MAYA",
            "4ucmqplxb029",
            "112233445566",
            "PMP",
            "PHP",
            "127.0.0.1");
        var withdrawResponse = new FuKaiPayWithdrawResponse(formId, "2cc5c6b4-1f16-4c23-a353-b3f8ae8be183");

        var applyInfo = new FuKaiPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, FuKaiPayUtility.SerializerOptions)
            });

        var request = """
            {
                "Status": 1,
                "MerchantAccount": "merchantId",
                "ChannelCode": "MAYA",
                "SuccessUrl": "",
                "MerchantOrderNo": "2cc5c6b4-1f16-4c23-a353-b3f8ae8be183",
                "OrderNo": "NIXP4529278814513287168",
                "PaymentAmount": "100.00",
                "Amount": "100.00",
                "ExtendFields": "",
                "PaymentTime": "2024-05-21 13:39:00",
                "Remark": "",
                "Sign": "yc6o4xSpp354WFt0ng1e0GhdX0jCUUZ1dKzFkrQI3gQ="
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        var paymentReturn = new FuKaiPayWithdrawReturn()
        {
            FormId = formId,
            OrderAmount = decimal.Parse("100.00"),
            Sign = "yc6o4xSpp354WFt0ng1e0GhdX0jCUUZ1dKzFkrQI3gQ=",
            Status = "1",
            PaymentTime = "2024-05-21 13:39:00",
            MerchantAccount = "merchantId",
            ChannelCode = "MAYA",
            Remark = "",
            ThirdPartyOrderNo = "NIXP4529278814513287168",
            RealAmount = decimal.Parse("100.00"),
        };

        Assert.Equal("success", actual);
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, FuKaiPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .PaidWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }
}