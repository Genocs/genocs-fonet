using Genocs.Fonet.WebApi.Models;

namespace Genocs.Fonet.WebApi.Services;

public interface IPdfBuildService
{
    Task<PdfBuildResult> BuildAsync(PrintPdfRequest request, CancellationToken cancellationToken = default);
}

public sealed class PdfBuildResult
{
    public required Stream Content { get; init; }
    public required string FileName { get; init; }
    public required string JobId { get; init; }
    public long SizeBytes { get; init; }
}
