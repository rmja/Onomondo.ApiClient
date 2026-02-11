using System.Text.Json.Serialization;
using Onomondo.ApiClient.Sims;
using Onomondo.ApiClient.Tags;

namespace Onomondo.ApiClient.Internal;

[JsonSerializable(typeof(Sim))]
[JsonSerializable(typeof(SimPage))]
[JsonSerializable(typeof(SimPatch))]
[JsonSerializable(typeof(Tag))]
[JsonSerializable(typeof(TagPage))]
[JsonSerializable(typeof(Technologies))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    Converters = [typeof(OptionJsonConverter<string>), typeof(OptionJsonConverter<List<string>>)]
)]
internal partial class OnomondoApiJsonSerializerContext : JsonSerializerContext;
