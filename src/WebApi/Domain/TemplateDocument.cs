using Genocs.Core.Domain.Repositories;
using Genocs.Persistence.MongoDB.Domain.Entities;
using MongoDB.Bson;

namespace Genocs.Fonet.WebApi.Domain;

[TableMapping("templates")]
public sealed class TemplateDocument : IMongoEntity
{
    public ObjectId Id { get; set; }

    /// <summary>Business key used by the API (e.g. "books").</summary>
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>File name under the templates volume (e.g. "books.fo").</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Optional localization resources file name.</summary>
    public string? ResourcesName { get; set; }

    /// <summary>Optional culture / country id for localized resources.</summary>
    public string? CountryId { get; set; }

    /// <summary>How to materialize the request model into <c>IPrintableDocument</c>.</summary>
    public string ModelType { get; set; } = "Json";

    public string? Description { get; set; }

    public bool Active { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsTransient() => Id == ObjectId.Empty;
}
