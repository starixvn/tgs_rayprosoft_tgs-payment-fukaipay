using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NATS.Client.Core;
using Tgs.Payment.Clients;
using Tgs.Payment.FuKaiPay;
using Tgs.Payment.FuKaiPay.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPaymentClient()
    .AddKeyedSingleton("Nats",
    (sp, _) => new NatsConnection(NatsOpts.Default with
    {
        Url = "nats://localhost:4222"
    }))
    .AddPhoenixToolkitsMessageQueue()
    .AddNatsMessageQueue(natsConfig => natsConfig
        .ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats")))
    .AddNatsGlobPatternExchange("*");

var app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var paymentClient = app.Services.GetRequiredService<IPaymentClient>();

var merchantId = "merchantId";
var merchantKey = "merchantKey";

//代收GCASH
//var orderNo = Guid.NewGuid();
//var amount = 800m;

//var depositsettings = new FuKaiPayDepositSettings(
//    merchantId,
//    merchantKey,
//    orderNo,
//    "GCASH",
//    "4ucmqplxb029",
//    "PHP",
//    "127.0.0.1");

//var depositreturn = await paymentClient.DepositAsync(
//    "tgs.payment.fukaipay.deposit",
//    amount,
//    JsonSerializer.Serialize(depositsettings));
//logger.LogInformation($"deposit return: {depositreturn}");

//代付
//var orderNo = Guid.NewGuid();
//var withdrawSettings = new FuKaiPayWithdrawSettings(
//    merchantId,
//    merchantKey,
//    orderNo,
//    "MAYA",
//    "4ucmqplxb029",
//    "112233445566",
//    "PMP",
//    "PHP",
//    "127.0.0.1");

//var withdrawReturn = await paymentClient.WithdrawAsync(
//    "tgs.payment.fukaipay.withdraw",
//    100M,
//    JsonSerializer.Serialize(withdrawSettings));
//logger.LogInformation($"Withdraw Return: {withdrawReturn}");

//代收訂單查詢
//var queryFormId = Guid.Parse("b667ff5c-b142-4460-9a52-bab2bf4617e4");

//var httpClient = new HttpClient
//{
//    BaseAddress = new Uri("https://openapi.fukuipay.ooo")
//};
//var client = new FuKaiPayClient(
//    httpClient, 
//    new FuKaiPayHasher(NullLogger<FuKaiPayHasher>.Instance),
//    TimeProvider.System, 
//    NullLogger<FuKaiPayClient>.Instance);
//var depositQuery = await client.QueryDepositFormAsync(merchantId, merchantKey, queryFormId);

//logger.LogInformation("{depositQuery}", depositQuery);