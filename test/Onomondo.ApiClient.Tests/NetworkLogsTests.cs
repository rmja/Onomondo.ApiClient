namespace Onomondo.ApiClient.Tests;

public class NetworkLogsTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly IOnomondoApiClient _client = fixture.Client;

    [Fact]
    public async Task CanGetNetworkLogs()
    {
        var timestamp = DateTime.UtcNow.AddDays(-20);
        var logs = await _client
            .NetworkLogs.EnumerateSimNetworkLogsAsync("000869117", timestamp)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.NotEmpty(logs);
    }
}
