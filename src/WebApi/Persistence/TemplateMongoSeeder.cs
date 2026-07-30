using Genocs.Fonet.WebApi.Domain;
using Genocs.Persistence.MongoDB.Repositories;
using MongoDB.Driver;

namespace Genocs.Fonet.WebApi.Persistence;

/// <summary>
/// Seeds the sample <c>books</c> template metadata used by the Host demo pipeline.
/// Template files themselves live on the external volume under pdfStorage.templatesPath.
/// </summary>
public sealed class TemplateMongoSeeder : IMongoSeeder
{
    public async Task SeedAsync(IMongoDatabase database, CancellationToken cancellationToken = default)
    {
        var templates = database.GetCollection<TemplateDocument>("templates");

        var existing = await templates
            .Find(t => t.TemplateId == "books")
            .AnyAsync(cancellationToken);

        if (existing)
        {
            return;
        }

        await templates.InsertOneAsync(
            new TemplateDocument
            {
                TemplateId = "books",
                FileName = "books.xslt",
                ModelType = "Books",
                Description = "Host sample books catalog template",
                Active = true,
                CreatedAtUtc = DateTime.UtcNow
            },
            cancellationToken: cancellationToken);
    }
}
