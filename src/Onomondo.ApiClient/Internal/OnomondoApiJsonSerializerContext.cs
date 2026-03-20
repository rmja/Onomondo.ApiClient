using System.Text.Json;
using System.Text.Json.Serialization;
using Onomondo.ApiClient.NetworkLogs;
using Onomondo.ApiClient.Sims;
using Onomondo.ApiClient.Tags;

namespace Onomondo.ApiClient.Internal;

[JsonSerializable(typeof(Sim))]
[JsonSerializable(typeof(SimPage))]
[JsonSerializable(typeof(SimPatch))]
[JsonSerializable(typeof(Tag))]
[JsonSerializable(typeof(TagPage))]
[JsonSerializable(typeof(Technologies))]
[JsonSerializable(typeof(NetworkLogPage))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    Converters = [
        typeof(OptionJsonConverter<string>),
        typeof(OptionJsonConverter<List<string>>),
        typeof(KebabCaseLowerJsonStringEnumConverter<LogType>),
    ]
)]
internal partial class OnomondoApiJsonSerializerContext : JsonSerializerContext;

class KebabCaseLowerJsonStringEnumConverter<TEnum>()
    : JsonStringEnumConverter<TEnum>(JsonNamingPolicy.KebabCaseLower)
    where TEnum : struct, Enum;
