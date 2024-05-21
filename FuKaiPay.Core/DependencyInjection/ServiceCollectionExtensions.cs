using System.Reflection;
using Tgs.Payment.FuKaiPay;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFuKaiPay(
        this IServiceCollection services,
        Action<FuKaiPayOptions, IServiceProvider> configureOptions,
        Action<IHttpClientBuilder> ConfigureAiClient)
    {
        ConfigureAiClient?.Invoke(services.AddHttpClient<FuKaiPayClient>());

        return services
            .AddOptions<FuKaiPayOptions>()
            .Configure(configureOptions)
            .Services
            .RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
            .AddSingleton<FuKaiPayHasher>();
    }
}