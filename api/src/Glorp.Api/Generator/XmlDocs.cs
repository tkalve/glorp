using System.Reflection;
using System.Xml.Linq;

namespace Glorp.Api.Generator;

/// <summary>
/// Loads the assembly's XML documentation file and exposes summaries
/// for types, properties, and record positional parameters.
/// </summary>
public sealed class XmlDocs
{
    private readonly Dictionary<string, string> _summaries = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<string, string>> _typeParams = new(StringComparer.Ordinal);

    public static XmlDocs LoadFor(Assembly assembly)
    {
        var xmlPath = Path.ChangeExtension(assembly.Location, ".xml");
        return File.Exists(xmlPath) ? new XmlDocs(xmlPath) : new XmlDocs();
    }

    private XmlDocs() { }

    private XmlDocs(string xmlPath)
    {
        var doc = XDocument.Load(xmlPath);
        foreach (var member in doc.Descendants("member"))
        {
            var name = (string?)member.Attribute("name");
            if (name is null) continue;

            var summary = Normalize((string?)member.Element("summary"));
            if (!string.IsNullOrEmpty(summary))
            {
                _summaries[name] = summary;
            }

            if (name.StartsWith("T:", StringComparison.Ordinal))
            {
                foreach (var p in member.Elements("param"))
                {
                    var paramName = (string?)p.Attribute("name");
                    var text = Normalize(p.Value);
                    if (string.IsNullOrEmpty(paramName) || string.IsNullOrEmpty(text)) continue;
                    if (!_typeParams.TryGetValue(name, out var bucket))
                    {
                        bucket = new Dictionary<string, string>(StringComparer.Ordinal);
                        _typeParams[name] = bucket;
                    }
                    bucket[paramName] = text;
                }
            }
        }
    }

    public string? GetTypeSummary(Type t) =>
        _summaries.TryGetValue($"T:{TypeKey(t)}", out var s) ? s : null;

    public string? GetPropertySummary(Type owner, string propertyName)
    {
        var key = $"P:{TypeKey(owner)}.{propertyName}";
        if (_summaries.TryGetValue(key, out var s)) return s;

        // Record positional parameters are documented via <param> on the type.
        if (_typeParams.TryGetValue($"T:{TypeKey(owner)}", out var bucket) &&
            bucket.TryGetValue(propertyName, out var paramSummary))
        {
            return paramSummary;
        }

        return null;
    }

    private static string TypeKey(Type t) => t.FullName?.Replace('+', '.') ?? t.Name;

    private static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var lines = raw.Replace("\r\n", "\n").Split('\n');
        var trimmed = lines.Select(l => l.Trim()).Where(l => l.Length > 0);
        return string.Join(" ", trimmed);
    }
}
