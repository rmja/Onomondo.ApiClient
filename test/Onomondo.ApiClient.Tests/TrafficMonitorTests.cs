using Microsoft.Extensions.DependencyInjection;

namespace Onomondo.ApiClient.Tests;

public sealed class TrafficMonitorTests : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private readonly TrafficMonitor _monitor;

    public TrafficMonitorTests(ApiFixture fixture)
    {
        var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        _monitor = ActivatorUtilities.CreateInstance<TrafficMonitor>(services, fixture.ApiKey);
    }

    public async ValueTask InitializeAsync()
    {
        await _monitor.ConnectAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _monitor.DisconnectAsync(TestContext.Current.CancellationToken);
        await _monitor.DisposeAsync();
    }

    [Theory]
    [InlineData("000868942")]
    public async Task CanMonitorSingleSim(string simId)
    {
        await using var subscription = await _monitor.SubscribeAsync(
            simId,
            TestContext.Current.CancellationToken
        );
        await foreach (var packet in subscription)
        {
            Assert.Equal(simId, packet.SimId);
            Assert.NotEmpty(packet.PacketBytes);
            break;
        }
    }

    [Fact]
    public async Task CanMonitorMultipleSims()
    {
        var packetCount = new Dictionary<string, int>();
        await using var subscription = await _monitor.SubscribeAsync(
            ["000868942", "000868955"],
            TestContext.Current.CancellationToken
        );
        await foreach (var packet in subscription)
        {
            packetCount[packet.SimId] = packetCount.GetValueOrDefault(packet.SimId) + 1;
            Assert.NotEmpty(packet.PacketBytes);
            if (packetCount.Count == 2)
            {
                break;
            }
        }
    }

    [Theory]
    [InlineData("000868942")]
    public async Task CanMonitorSingleSimMultipleTimes(string simId)
    {
        await using (
            var firstSubscription = await _monitor.SubscribeAsync(
                simId,
                TestContext.Current.CancellationToken
            )
        )
        {
            var cts = new CancellationTokenSource(100);
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await foreach (var packet in firstSubscription.WithCancellation(cts.Token)) { }
            });
        }

        await using (
            var secondSubscription = await _monitor.SubscribeAsync(
                simId,
                TestContext.Current.CancellationToken
            )
        )
        {
            await foreach (var packet in secondSubscription)
            {
                break;
            }
        }

        await Task.Delay(1000, TestContext.Current.CancellationToken);
    }
}
