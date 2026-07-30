using System.Text.Json;

namespace Genocs.Fonet.WebApi.Models;

public sealed class PrintPdfRequest
{
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>Document model consumed by the XSLT template.</summary>
    public JsonElement Model { get; set; }

    /// <summary>Optional override for localization country id.</summary>
    public string? CountryId { get; set; }
}
