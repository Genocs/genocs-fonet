using Genocs.Common.CQRS.Queries;
using Genocs.Fonet.WebApi.Models;

namespace Genocs.Fonet.WebApi.Services;

public interface IPdfJobQueryService
{
    Task<PagedResult<PdfJobResponse>> BrowseAsync(BrowsePdfJobsQuery query, CancellationToken cancellationToken = default);
}
