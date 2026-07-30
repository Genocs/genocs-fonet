using System.Text.Json;
using System.Xml.Linq;
using Genocs.Fonet.XsltTransformer.Transformers;

namespace Genocs.Fonet.WebApi.Models;

/// <summary>
/// Generic printable document that converts a JSON model into XML elements
/// (PascalCase names) so XSLT can consume arbitrary payloads.
/// </summary>
public sealed class JsonPrintableDocument : IPrintableDocument
{
    private readonly JsonElement _model;
    private readonly string _rootElementName;

    public JsonPrintableDocument(JsonElement model, string rootElementName = "Document")
    {
        _model = model;
        _rootElementName = string.IsNullOrWhiteSpace(rootElementName) ? "Document" : rootElementName;
    }

    public string? DocumentName { get; set; }

    public string ToXml()
    {
        var root = ToXElement(_model, _rootElementName);
        if (!string.IsNullOrWhiteSpace(DocumentName))
        {
            root.Add(new XElement("DocumentName", DocumentName));
        }

        return root.ToString(SaveOptions.DisableFormatting);
    }

    private static XElement ToXElement(JsonElement element, string name)
    {
        var safeName = SanitizeName(name);

        return element.ValueKind switch
        {
            JsonValueKind.Object => BuildObject(element, safeName),
            JsonValueKind.Array => BuildArray(element, safeName),
            JsonValueKind.String => new XElement(safeName, element.GetString()),
            JsonValueKind.Number => new XElement(safeName, element.GetRawText()),
            JsonValueKind.True => new XElement(safeName, "true"),
            JsonValueKind.False => new XElement(safeName, "false"),
            JsonValueKind.Null => new XElement(safeName),
            _ => new XElement(safeName, element.GetRawText())
        };
    }

    private static XElement BuildObject(JsonElement element, string name)
    {
        var node = new XElement(name);
        foreach (var property in element.EnumerateObject())
        {
            node.Add(ToXElement(property.Value, ToPascalCase(property.Name)));
        }

        return node;
    }

    private static XElement BuildArray(JsonElement element, string name)
    {
        var node = new XElement(name);
        var itemName = name.EndsWith('s') && name.Length > 1
            ? name[..^1]
            : "Item";

        foreach (var item in element.EnumerateArray())
        {
            node.Add(ToXElement(item, itemName));
        }

        return node;
    }

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Value";
        }

        return char.ToUpperInvariant(value[0]) + value[1..];
    }

    private static string SanitizeName(string name)
    {
        var cleaned = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return "Value";
        }

        if (char.IsDigit(cleaned[0]))
        {
            cleaned = "_" + cleaned;
        }

        return cleaned;
    }
}
