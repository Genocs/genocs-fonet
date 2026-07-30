using Genocs.Core.Builders;
using Genocs.Fonet.WebApi.Configuration;
using Genocs.Fonet.WebApi.Exceptions;
using Genocs.Fonet.WebApi.Models;
using Genocs.Fonet.WebApi.Persistence;
using Genocs.Fonet.WebApi.Services;
using Genocs.Fonet.XsltTransformer.Transformers;
using Genocs.Logging;
using Genocs.Persistence.MongoDB.Extensions;
using Genocs.WebApi;
using Genocs.WebApi.OpenApi;
using Microsoft.OpenApi;
using Serilog;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLogging();

builder.Services.Configure<PdfStorageOptions>(
    builder.Configuration.GetSection(PdfStorageOptions.SectionName));

builder.Services.AddSingleton<IPdfWriterService, XslFoPdfService>();
builder.Services.AddScoped<IPdfBuildService, PdfBuildService>();

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddErrorHandler<ExceptionToResponseMapper>()
    .AddMongo(seederType: typeof(TemplateMongoSeeder))
    .AddMongoWithRegistration()
    .AddWebApi()
    .AddOpenApiDocs();

var app = builder.Build();
genocs.Build(app.Services);

app.UseGenocs();
app.UseErrorHandler();
app.UseOpenApiDocs();

app.UseEndpoints(static endpoints =>
{
    endpoints.Get("health", async context =>
        await context.Response.Ok(new { status = "ok", service = "Genocs.Fonet.WebApi" }));

    BuildPdf(endpoints);
});

//app.MapDefaultEndpoints();
app.Run();

Log.CloseAndFlush();

static void BuildPdf(IEndpointsBuilder endpoints)
{
    endpoints.Post<PrintPdfRequest>("api/pdf", async (request, context) =>
    {
        var pdfBuild = context.RequestServices.GetRequiredService<IPdfBuildService>();
        var result = await pdfBuild.BuildAsync(request, context.RequestAborted);

        context.Response.Headers.Append("X-Pdf-Job-Id", result.JobId);
        context.Response.ContentType = "application/pdf";
        context.Response.Headers.ContentDisposition = $"attachment; filename=\"{result.FileName}\"";
        context.Response.StatusCode = StatusCodes.Status200OK;

        await result.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
        await result.Content.DisposeAsync();
    });
}