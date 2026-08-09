using System.Linq.Expressions;
using Genocs.Common.CQRS.Queries;
using Genocs.Fonet.WebApi.Domain;
using Genocs.Fonet.WebApi.Models;
using Genocs.Persistence.MongoDB.Domain.Repositories;

namespace Genocs.Fonet.WebApi.Services;

public sealed class PdfJobQueryService(IMongoRepository<PdfJobDocument> jobs) : IPdfJobQueryService
{
    private const string CompletedStatus = "Completed";

    public async Task<PagedResult<PdfJobResponse>> BrowseAsync(
        BrowsePdfJobsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        NormalizePaging(query);

        var predicate = BuildPredicate(query);
        var page = await jobs.BrowseAsync(predicate, query, cancellationToken);

        var items = page.Items.Select(Map);
        return PagedResult<PdfJobResponse>.From(page, items);
    }

    private static void NormalizePaging(BrowsePdfJobsQuery query)
    {
        if (query.Page < 0)
        {
            query.Page = 0;
        }

        if (query.Results <= 0)
        {
            query.Results = 10;
        }
        else if (query.Results > 100)
        {
            query.Results = 100;
        }

        if (string.IsNullOrWhiteSpace(query.OrderBy))
        {
            query.OrderBy = nameof(PdfJobDocument.CreatedAtUtc);
            query.SortOrder ??= "desc";
        }
        else if (string.IsNullOrWhiteSpace(query.SortOrder))
        {
            query.SortOrder = "desc";
        }
    }

    private static Expression<Func<PdfJobDocument, bool>> BuildPredicate(BrowsePdfJobsQuery query)
    {
        string? status = NormalizeStatusFilter(query.Status);
        string? templateId = string.IsNullOrWhiteSpace(query.TemplateId)
            ? null
            : query.TemplateId.Trim();

        if (status is null && templateId is null)
        {
            return _ => true;
        }

        if (status is not null && templateId is not null)
        {
            return job => job.Status == status && job.TemplateId == templateId;
        }

        if (status is not null)
        {
            return job => job.Status == status;
        }

        return job => job.TemplateId == templateId;
    }

    /// <summary>
    /// Returns the status to filter by, or <c>null</c> when no status filter should be applied.
    /// Defaults to completed jobs (processed documents) when the client omits <see cref="BrowsePdfJobsQuery.Status"/>.
    /// </summary>
    private static string? NormalizeStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return CompletedStatus;
        }

        string trimmed = status.Trim();
        if (trimmed is "*" or "all" || trimmed.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return trimmed;
    }

    private static PdfJobResponse Map(PdfJobDocument job) => new()
    {
        Id = job.Id.ToString(),
        TemplateId = job.TemplateId,
        Status = job.Status,
        DocumentName = job.DocumentName,
        SizeBytes = job.SizeBytes,
        Error = job.Error,
        CreatedAtUtc = job.CreatedAtUtc,
        CompletedAtUtc = job.CompletedAtUtc
    };
}
