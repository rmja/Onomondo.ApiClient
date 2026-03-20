namespace Onomondo.ApiClient.Tests;

public class NetworkLogsTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly IOnomondoApiClient _client = fixture.Client;

    [Fact]
    public async Task CanGetNetworkLogs()
    {
        var logs = await _client
            .NetworkLogs.EnumerateSimNetworkLogsAsync(
                "000869117",
                new(2026, 03, 20, 00, 00, 00, TimeSpan.Zero)
            )
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.NotEmpty(logs);
    }
}
