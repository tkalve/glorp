using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glorp.Api.Json;

/// <summary>
/// Handles polymorphic deserialization of IGlorpRequest&lt;T&gt; and IGlorpResponse&lt;T&gt;
/// typed variables by dispatching on the "$type" discriminator.
/// </summary>
public class InterfaceConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        Configuration.IsGlorpInterfaceType(typeToConvert);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(InterfaceConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}
