using Genocs.Core.Domain.Repositories;
using Genocs.Persistence.MongoDB.Domain.Entities;
using MongoDB.Bson;

namespace Genocs.Fonet.WebApi.Domain;


/// <summary>
/// Represents a PDF job document in the MongoDB collection "pdf_jobs".
/// </summary>
[TableMapping("pdf_jobs")]
public sealed class PdfJobDocument : IMongoEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the PDF job document.
    /// </summary>
    public ObjectId Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the template used for generating the PDF.
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status of the PDF job document. Possible values are "Pending", "InProgress", "Completed", or "Failed".
    /// </summary>
    public string Status { get; set; } = "Pending";

    public string? DocumentName { get; set; }

    public long SizeBytes { get; set; }

    public string? Error { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }

    public bool IsTransient() => Id == ObjectId.Empty;
}
