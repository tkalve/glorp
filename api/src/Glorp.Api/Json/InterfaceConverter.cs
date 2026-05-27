using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glorp.Api.Json;

public class InterfaceConverter<T> : JsonConverter<T>
{
    private static readonly IReadOnlyDictionary<string, Type> TypeRegistry =
        Configuration.GetTypeRegistry();

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("$type", out var typeDiscriminator) ||
            typeDiscriminator.GetString() is not string typeName ||
            !TypeRegistry.TryGetValue(typeName, out var concreteType))
        {
            throw new JsonException("Cannot deserialize: missing or unknown \"$type\" discriminator.");
        }

        return (T?)JsonSerializer.Deserialize(root.GetRawText(), concreteType, options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, value!.GetType(), options);
}
