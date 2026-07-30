using System.Text.Json;
using Genocs.Fonet.WebApi.Configuration;
using Genocs.Fonet.WebApi.Domain;
using Genocs.Fonet.WebApi.Models;
using Genocs.Fonet.XsltTransformer.Transformers;
using Genocs.Persistence.MongoDB.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace Genocs.Fonet.WebApi.Services;

public sealed class PdfBuildService(
    IPdfWriterService pdfWriter,
    IMongoRepository<TemplateDocument> templates,
    IMongoRepository<PdfJobDocument> jobs,
    IOptions<PdfStorageOptions> storageOptions,
    ILogger<PdfBuildService> logger) : IPdfBuildService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<PdfBuildResult> BuildAsync(PrintPdfRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.TemplateId))
        {
            throw new ArgumentException("templateId is required.", nameof(request));
        }

        if (request.Model.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            throw new ArgumentException("model is required.", nameof(request));
        }

        var storage = storageOptions.Value;
        var template = await ResolveTemplateAsync(request.TemplateId, cancellationToken);

        var templatePath = Path.Combine(storage.ResolveTemplatesPath(), template.FileName);
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException(
                $"Template file '{template.FileName}' was not found under '{storage.ResolveTemplatesPath()}'.",
                templatePath);
        }

        var fontsDirectory = storage.ResolveFontsPath();
        if (!Directory.Exists(fontsDirectory))
        {
            throw new DirectoryNotFoundException($"Fonts directory '{fontsDirectory}' was not found.");
        }

        var document = CreatePrintableDocument(template, request.Model);
        var job = new PdfJobDocument
        {
            TemplateId = template.TemplateId,
            DocumentName = document.DocumentName,
            Status = "Processing",
            CreatedAtUtc = DateTime.UtcNow
        };

        await jobs.AddAsync(job, cancellationToken);

        try
        {
            // Mirror Host/XsltDriver: XslFoPdfService.Print with absolute template + fonts paths.
            var pdfStream = pdfWriter.Print(
                document: document,
                templateName: templatePath,
                resourcesName: ResolveResourcesPath(storage, template.ResourcesName),
                fontsDirectory: fontsDirectory,
                countryId: request.CountryId ?? template.CountryId);

            if (pdfStream.CanSeek)
            {
                pdfStream.Position = 0;
            }

            var size = pdfStream.CanSeek ? pdfStream.Length : 0L;
            job.Status = "Completed";
            job.SizeBytes = size;
            job.CompletedAtUtc = DateTime.UtcNow;
            await jobs.UpdateAsync(job, j => j.Id == job.Id, cancellationToken);

            logger.LogInformation(
                "Generated PDF for template {TemplateId}. Job {JobId}, size {SizeBytes} bytes.",
                template.TemplateId,
                job.Id,
                size);

            return new PdfBuildResult
            {
                Content = pdfStream,
                FileName = $"{template.TemplateId}-{job.Id}.pdf",
                JobId = job.Id.ToString(),
                SizeBytes = size
            };
        }
        catch (Exception ex)
        {
            job.Status = "Failed";
            job.Error = ex.Message;
            job.CompletedAtUtc = DateTime.UtcNow;
            await jobs.UpdateAsync(job, j => j.Id == job.Id, cancellationToken);
            throw;
        }
    }

    private async Task<TemplateDocument> ResolveTemplateAsync(string templateId, CancellationToken cancellationToken)
    {
        var matches = await templates.FindAsync(
            t => t.TemplateId == templateId && t.Active,
            cancellationToken);

        var template = matches.FirstOrDefault();
        if (template is null)
        {
            throw new KeyNotFoundException($"Active template '{templateId}' was not found.");
        }

        return template;
    }

    private static IPrintableDocument CreatePrintableDocument(TemplateDocument template, JsonElement model)
    {
        if (string.Equals(template.ModelType, "Books", StringComparison.OrdinalIgnoreCase))
        {
            var books = model.Deserialize<BooksDocument>(JsonOptions)
                ?? throw new InvalidOperationException("Unable to deserialize the model as a Books document.");

            books.DocumentName ??= template.TemplateId;
            return books;
        }

        var documentName = TryReadDocumentName(model) ?? template.TemplateId;
        return new JsonPrintableDocument(model, rootElementName: "Document")
        {
            DocumentName = documentName
        };
    }

    private static string? TryReadDocumentName(JsonElement model)
    {
        if (model.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (model.TryGetProperty("documentName", out var camel)
            && camel.ValueKind == JsonValueKind.String)
        {
            return camel.GetString();
        }

        if (model.TryGetProperty("DocumentName", out var pascal)
            && pascal.ValueKind == JsonValueKind.String)
        {
            return pascal.GetString();
        }

        return null;
    }

    private static string? ResolveResourcesPath(PdfStorageOptions storage, string? resourcesName)
    {
        if (string.IsNullOrWhiteSpace(resourcesName))
        {
            return null;
        }

        return Path.IsPathRooted(resourcesName)
            ? resourcesName
            : Path.Combine(storage.ResolveTemplatesPath(), resourcesName);
    }
}
