using Microsoft.Extensions.Options;
using Onomondo.ApiClient;

namespace Microsoft.Extensions.DependencyInjection;

public static class OnomondoApiExtensions
{
    public static IServiceCollection AddOnomondoApiClient(
        this IServiceCollection services,
        Action<OnomondoApiOptions> configureOptions
    )
    {
        services.AddHttpClient<OnomondoApiClient>();
        services.AddTransient<IOnomondoApiClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<OnomondoApiOptions>>();
            var apiKey = options.Value.ApiKey;
            return ActivatorUtilities.CreateInstance<OnomondoApiClient>(provider, apiKey);
        });
        services.Configure(configureOptions);
        return services;
    }
}
