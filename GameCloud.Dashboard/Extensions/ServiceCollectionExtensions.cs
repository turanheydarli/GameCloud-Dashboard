using GameCloud.Dashboard.Security;
using Refit;

namespace GameCloud.Dashboard.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRefitClient<T>(this IServiceCollection services, string baseUrl) where T : class
    {
        services.AddRefitClient<T>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<AuthenticationMessageHandler>();

        return services;
    }
}