using System.Text.Json;

namespace Genocs.Fonet.WebApi.Models;

/// <summary>
/// Request body for <c>POST /api/pdf</c>. Selects a template and supplies the printable model used to generate a PDF.
/// </summary>
public sealed class PrintPdfRequest
{
    /// <summary>Template identifier resolved from MongoDB (<c>templates</c> collection).</summary>
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>Document model consumed by the XSLT template.</summary>
    public JsonElement Model { get; set; }

    /// <summary>Optional override for localization country id.</summary>
    public string? CountryId { get; set; }
}
