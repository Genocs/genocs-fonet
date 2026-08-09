using Genocs.Common.CQRS.Queries;
using Genocs.Core.Builders;
using Genocs.Fonet.WebApi.Configuration;
using Genocs.Fonet.WebApi.Exceptions;
using Genocs.Fonet.WebApi.Models;
using Genocs.Fonet.WebApi.OpenApi;
using Genocs.Fonet.WebApi.Persistence;
using Genocs.Fonet.WebApi.Services;
using Genocs.Fonet.XsltTransformer.Transformers;
using Genocs.Logging;
using Genocs.Persistence.MongoDB.Extensions;
using Genocs.WebApi;
using Genocs.WebApi.OpenApi;
using Serilog;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLogging();

builder.Services.Configure<PdfStorageOptions>(
    builder.Configuration.GetSection(PdfStorageOptions.SectionName));

builder.Services.AddSingleton<IPdfWriterService, XslFoPdfService>();
builder.Services.AddScoped<IPdfBuildService, PdfBuildService>();
builder.Services.AddScoped<IPdfJobQueryService, PdfJobQueryService>();

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddErrorHandler<ExceptionToResponseMapper>()
    .AddMongo(seederType: typeof(TemplateMongoSeeder))
    .AddMongoWithRegistration()
    .AddWebApi()
    .AddOpenApiDocs();

builder.Services.ConfigureSwaggerGen(options =>
    options.DocumentFilter<EndpointDescriptionsDocumentFilter>());

var app = builder.Build();
genocs.Build(app.Services);

app.UseGenocs();
app.UseErrorHandler();
app.UseOpenApiDocs();

app.UseEndpoints(static endpoints =>
{
    endpoints.Get(
        "health",
        async context =>
            await context.Response.Ok(new { status = "ok", service = "Genocs.Fonet.WebApi" }),
        endpoint: route => route
            .WithSummary("Health check")
            .WithDescription(
                "Returns a lightweight liveness payload for the Genocs.Fonet.WebApi host. " +
                "Use this endpoint for container/orchestrator probes and basic connectivity checks.")
            .WithTags("System"));

    BuildPdf(endpoints);
    BrowsePdfJobs(endpoints);
});

//app.MapDefaultEndpoints();
app.Run();

Log.CloseAndFlush();

static void BuildPdf(IEndpointsBuilder endpoints)
{
    endpoints.Post<PrintPdfRequest>(
        "api/pdf",
        async (request, context) =>
        {
            var pdfBuild = context.RequestServices.GetRequiredService<IPdfBuildService>();
            var result = await pdfBuild.BuildAsync(request, context.RequestAborted);

            context.Response.Headers.Append("X-Pdf-Job-Id", result.JobId);
            context.Response.ContentType = "application/pdf";
            context.Response.Headers.ContentDisposition = $"attachment; filename=\"{result.FileName}\"";
            context.Response.StatusCode = StatusCodes.Status200OK;

            await result.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
            await result.Content.DisposeAsync();
        },
        endpoint: route =>
        {
            route
                .WithSummary("Build PDF")
                .WithDescription(
                    "Builds a PDF from an XSLT template and printable model. " +
                    "On success the response body is application/pdf and the job id is returned in X-Pdf-Job-Id.")
                .WithTags("PDF");

            if (route is RouteHandlerBuilder handler)
            {
                handler
                    .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
                    .Produces(StatusCodes.Status400BadRequest)
                    .Produces(StatusCodes.Status404NotFound);
            }
        });
}

static void BrowsePdfJobs(IEndpointsBuilder endpoints)
{
    endpoints.Get<BrowsePdfJobsQuery, PagedResult<PdfJobResponse>>(
        "api/pdf/jobs",
        async (query, context) =>
        {
            var pdfJobs = context.RequestServices.GetRequiredService<IPdfJobQueryService>();
            var result = await pdfJobs.BrowseAsync(query, context.RequestAborted);
            await context.Response.Ok(result);
        },
        endpoint: route =>
        {
            route
                .WithSummary("List processed PDF jobs")
                .WithDescription(
                    "Returns a paginated list of PDF jobs from MongoDB. " +
                    "Defaults to completed jobs; use status=all to include every status.")
                .WithTags("PDF");

            if (route is RouteHandlerBuilder handler)
            {
                handler.Produces<PagedResult<PdfJobResponse>>(StatusCodes.Status200OK);
            }
        });
}
