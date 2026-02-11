using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Onomondo.ApiClient.Tests;

public class ApiFixture
{
    public string ApiKey { get; set; }
    public IOnomondoApiClient Client { get; }

    public ApiFixture()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<ApiFixture>().Build();

        ApiKey =
            config["OnomondoApiKey"] ?? throw new Exception("No api configured as user secret");

        var services = new ServiceCollection()
            .AddLogging()
            .AddOnomondoApiClient(options => options.ApiKey = ApiKey)
            .BuildServiceProvider();
        Client = services.GetRequiredService<IOnomondoApiClient>();
    }
}
