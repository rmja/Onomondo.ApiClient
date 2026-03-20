using System.Globalization;
using System.Reflection;
using Onomondo.ApiClient.Internal;
using Refit;

namespace Onomondo.ApiClient;

public class OnomondoApiClient : IOnomondoApiClient
{
    private static readonly RefitSettings _refitSettings = new(
        new SystemTextJsonContentSerializer(OnomondoApiJsonSerializerOptions.Default),
        new OnomondoUrlParameterFormatter()
    );

    private readonly IOnomondoApi _api;

    public INetworkLogs NetworkLogs => _api;
    public ISims Sims => _api;
    public ITags Tags => _api;

    public OnomondoApiClient(HttpClient httpClient, string apiKey)
    {
        httpClient.BaseAddress = new("https://api.onomondo.com");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiKey);
        _api = RestService.For<IOnomondoApi>(httpClient, _refitSettings);
    }
}

file class OnomondoUrlParameterFormatter : IUrlParameterFormatter
{
    private static DefaultUrlParameterFormatter _defaultFormatter = new();

    public string? Format(object? value, ICustomAttributeProvider attributeProvider, Type type)
    {
        if (value is DateTimeOffset dto)
        {
            return dto.UtcDateTime.ToString(
                "yyyy-MM-dd'T'HH:mm:ss.fffK",
                CultureInfo.InvariantCulture
            );
        }
        return _defaultFormatter.Format(value, attributeProvider, type);
    }
}
