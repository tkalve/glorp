using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Glorp.Api.Json;

/// <summary>
/// Adds "$type" property to concrete IGlorpRequest/IGlorpResponse types during serialization,
/// and silently ignores it on deserialization.
/// </summary>
public class TypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var typeInfo = base.GetTypeInfo(type, options);

        if (Configuration.IsConcreteGlorpType(type) &&
            typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            var typeName = type.Name;
            var prop = typeInfo.CreateJsonPropertyInfo(typeof(string), "$type");
            prop.Order = int.MinValue;
            prop.Get = _ => typeName;
            prop.Set = (_, _) => { };
            typeInfo.Properties.Add(prop);
        }

        return typeInfo;
    }
}
