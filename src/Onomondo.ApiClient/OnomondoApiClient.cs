using Onomondo.ApiClient.Internal;
using Onomondo.ApiClient.Sims;
using Onomondo.ApiClient.Tags;
using Refit;

namespace Onomondo.ApiClient;

public class OnomondoApiClient : IOnomondoApiClient
{
    private static readonly RefitSettings _refitSettings = new(
        new SystemTextJsonContentSerializer(OnomondoApiJsonSerializerOptions.Default)
    );

    private readonly IOnomondoApi _api;

    public ISims Sims => _api;
    public ITags Tags => _api;

    public OnomondoApiClient(HttpClient httpClient, string apiKey)
    {
        httpClient.BaseAddress = new("https://api.onomondo.com");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiKey);
        _api = RestService.For<IOnomondoApi>(httpClient, _refitSettings);
    }
}
