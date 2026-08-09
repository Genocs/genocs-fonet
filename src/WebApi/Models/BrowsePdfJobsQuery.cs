using Genocs.Common.CQRS.Queries;

namespace Genocs.Fonet.WebApi.Models;

/// <summary>
/// Paged query for PDF jobs stored in MongoDB (<c>pdf_jobs</c>).
/// </summary>
public sealed class BrowsePdfJobsQuery : PagedQueryBase
{
    /// <summary>
    /// Optional status filter. When omitted, only completed (processed) jobs are returned.
    /// Pass <c>*</c> or <c>all</c> to include every status.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Optional template identifier filter.
    /// </summary>
    public string? TemplateId { get; set; }
}
