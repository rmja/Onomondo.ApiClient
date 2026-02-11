using System.Text.Json;
using Onomondo.ApiClient.Internal;
using Onomondo.ApiClient.Sims;

namespace Onomondo.ApiClient.Tests;

public class SerializationTests
{
    [Fact]
    public void CanSerializePatch()
    {
        // Given
        var patch = new SimPatch() { Tags = new List<string>() { "tag" } };

        // When
        var json = JsonSerializer.Serialize(patch, OnomondoApiJsonSerializerOptions.Default);

        // Then
        Assert.Equal("""{"tags":["tag"]}""", json);
    }

    [Fact]
    public void CanDeserializePatch()
    {
        // Given
        var json = """{"tags": ["tag"]}""";

        // When
        var patch = JsonSerializer.Deserialize<SimPatch>(
            json,
            OnomondoApiJsonSerializerOptions.Default
        )!;

        // Then
        Assert.True(patch.Label.IsT0);
        Assert.Equal(["tag"], patch.Tags.AsT1);
    }
}
