using Onomondo.ApiClient.Sims;
using Refit;

namespace Onomondo.ApiClient.Tests;

public class SimsTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly IOnomondoApiClient _client = fixture.Client;

    private const string GettingStartedSimId = "000290304";

    [Fact]
    public async Task CanGetSimById()
    {
        var sim = await _client.Sims.GetSimByIdAsync("000868918");

        Assert.Equal("000868918", sim.Id);
        Assert.Equal("89457387300008689187", sim.Iccid);
    }

    [Fact]
    public async Task CanEnumerateSims()
    {
        var sims = await _client.Sims.EnumerateSimsAsync().ToListAsync();

        Assert.Equal(1716, sims.Count);
    }

    [Fact]
    public async Task CanUpdateSim()
    {
        var updatedLabel = "getting-started-" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var sim = await _client.Sims.UpdateSimAsync(
            GettingStartedSimId,
            new SimPatch { Label = updatedLabel }
        );

        Assert.Equal(updatedLabel, sim.Label);
    }

    [Fact]
    public async Task CanAddAndRemoveAndSetTag()
    {
        const string TestTagId = "6b6b0831-6e97-4bce-bc07-209030db8ae2";

        await _client.Sims.AddTagAsync(GettingStartedSimId, TestTagId);
        await _client.Sims.RemoveTagAsync(GettingStartedSimId, TestTagId);

        Assert.True(await _client.Sims.SetTagAsync(GettingStartedSimId, TestTagId));
        Assert.False(await _client.Sims.SetTagAsync(GettingStartedSimId, TestTagId));

        await _client.Sims.RemoveTagAsync(GettingStartedSimId, TestTagId);
    }

    [Fact]
    public async Task CannotAddTagIfAlreadyAdded()
    {
        const string PoCTagId = "1f788996-a2ae-4c0a-af4d-765c13b1c615";

        await Assert.ThrowsAsync<ApiException>(async () =>
            await _client.Sims.AddTagAsync(GettingStartedSimId, PoCTagId)
        );
    }
}
