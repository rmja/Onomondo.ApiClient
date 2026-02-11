using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using OneOf;
using OneOf.Types;

namespace Onomondo.ApiClient.Internal;

internal static class OnomondoApiJsonSerializerOptions
{
    public static JsonSerializerOptions Default { get; } =
        new(OnomondoApiJsonSerializerContext.Default.Options)
        {
            TypeInfoResolver =
                OnomondoApiJsonSerializerContext.Default.WithOneOfExcludeOptionNoneVariant(),
        };

    private static IJsonTypeInfoResolver WithOneOfExcludeOptionNoneVariant(
        this IJsonTypeInfoResolver typeInfoResolver
    ) => typeInfoResolver.WithAddedModifier(ExcludeOptionNoneVariant);

    private static void ExcludeOptionNoneVariant(JsonTypeInfo typeInfo)
    {
        // Do not serialize Option.None
        foreach (var property in typeInfo.Properties)
        {
            if (
                property.PropertyType.IsGenericType
                && property.PropertyType.GetGenericTypeDefinition() == typeof(OneOf<,>)
                && property.PropertyType.GetGenericArguments()[0] == typeof(None)
            )
            {
                property.ShouldSerialize = static (_, value) => IsSome(value!);
            }
        }

        static bool IsSome(object value)
        {
#pragma warning disable IL2072
            var defaultValue = Activator.CreateInstance(value.GetType());
#pragma warning restore IL2072
            return !value.Equals(defaultValue);
        }
    }
}
