namespace Genocs.Fonet.WebApi.Models;

/// <summary>
/// PDF job metadata returned by <c>GET /api/pdf/jobs</c>.
/// </summary>
public sealed class PdfJobResponse
{
    /// <summary>MongoDB job identifier (<c>ObjectId</c> as string).</summary>
    public required string Id { get; init; }

    /// <summary>Template used to generate the PDF.</summary>
    public required string TemplateId { get; init; }

    /// <summary>Job status (for example <c>Processing</c>, <c>Completed</c>, or <c>Failed</c>).</summary>
    public required string Status { get; init; }

    /// <summary>Optional document display name.</summary>
    public string? DocumentName { get; init; }

    /// <summary>Generated PDF size in bytes when available.</summary>
    public long SizeBytes { get; init; }

    /// <summary>Failure details when <see cref="Status"/> is <c>Failed</c>.</summary>
    public string? Error { get; init; }

    /// <summary>UTC timestamp when the job was created.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>UTC timestamp when the job finished, if completed or failed.</summary>
    public DateTime? CompletedAtUtc { get; init; }
}
