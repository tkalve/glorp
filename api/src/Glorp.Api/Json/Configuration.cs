using System.Reflection;
using Glorp.Api.Glorpiatr;

namespace Glorp.Api.Json;
    
public static class Configuration
{
    private static readonly Type[] GlorpGenericInterfaces =
        [typeof(IGlorpRequest<>), typeof(IGlorpResponse<>)];

    private static readonly IReadOnlyDictionary<string, Type> TypeRegistry =
        SafeGetTypes(Assembly.GetExecutingAssembly())
            .Where(IsConcreteGlorpType)
            .ToDictionary(t => t.Name);

    private static IEnumerable<Type> SafeGetTypes(Assembly asm)
    {
        try { return asm.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null)!; }
    }

    public static bool IsConcreteGlorpType(Type t) =>
        !t.IsAbstract && !t.IsInterface &&
        t.GetInterfaces().Any(i =>
            i.IsGenericType && GlorpGenericInterfaces.Contains(i.GetGenericTypeDefinition()));

    public static bool IsGlorpInterfaceType(Type t) =>
        t.IsInterface && t.IsGenericType &&
        GlorpGenericInterfaces.Contains(t.GetGenericTypeDefinition());

    public static IReadOnlyDictionary<string, Type> GetTypeRegistry() => TypeRegistry;
}
