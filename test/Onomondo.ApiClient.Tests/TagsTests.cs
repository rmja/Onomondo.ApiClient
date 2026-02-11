namespace Onomondo.ApiClient.Tests;

public class TagsTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly IOnomondoApiClient _client = fixture.Client;

    [Fact]
    public async Task CanEnumerateTags()
    {
        var tags = await _client.Tags.EnumerateTagsAsync().ToListAsync();

        Assert.True(tags.Count >= 43);
    }
}
