using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using OneOf;
using OneOf.Types;

namespace Onomondo.ApiClient.Internal;

internal class OptionJsonConverter<TSome> : JsonConverter<OneOf<None, TSome>>
{
    public override OneOf<None, TSome> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var someTypeInfo = (JsonTypeInfo<TSome>)options.GetTypeInfo(typeof(TSome));
        return JsonSerializer.Deserialize(ref reader, someTypeInfo)!;
    }

    public override void Write(
        Utf8JsonWriter writer,
        OneOf<None, TSome> value,
        JsonSerializerOptions options
    )
    {
        value.Switch(
            none =>
            {
                writer.WriteNullValue();
            },
            some =>
            {
                var someTypeInfo = (JsonTypeInfo<TSome>)options.GetTypeInfo(typeof(TSome));
                JsonSerializer.Serialize(writer, some, someTypeInfo);
            }
        );
    }
}
