using System.Collections;
using System.Text;
using Genocs.Fonet.Layout;
using Genocs.Fonet.Pdf;

namespace Genocs.Fonet.DataTypes;

internal class IDReferences
{
    private const int IdPagging = 5000;

    private readonly Hashtable _idReferences;
    private readonly Hashtable _idValidation;
    private readonly Hashtable _idUnvalidated;

    public IDReferences()
    {
        _idReferences = [];
        _idValidation = [];
        _idUnvalidated = [];
    }

    public void InitializeID(string id, Area area)
    {
        CreateID(id);
        ConfigureID(id, area);
    }

    public void AddToUnvalidatedIdList(string id)
        => _idUnvalidated[id] = "";

    public void RemoveFromUnvalidatedIDList(string id)
        => _idUnvalidated.Remove(id);

    public bool DoesUnvalidatedIDExist(string id)
        => _idUnvalidated.ContainsKey(id);

    public void AddToIdValidationList(string id)
        => _idValidation[id] = "";

    public void RemoveFromIdValidationList(string id)
        => _idValidation.Remove(id);

    public void RemoveID(string id)
        => _idReferences.Remove(id);

    public bool IsEveryIdValid()
        => _idValidation.Count == 0;
    public bool DoesIDExist(string id)
        => _idReferences.ContainsKey(id);

    public ICollection GetInvalidElements()
        => _idValidation.Keys;

    public void CreateID(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        if (DoesUnvalidatedIDExist(id))
        {
            RemoveFromUnvalidatedIDList(id);
            RemoveFromIdValidationList(id);
        }
        else if (DoesIDExist(id))
        {
            throw new FonetException($"The id '{id}' already exists in this document");
        }
        else
        {
            CreateNewId(id);
            RemoveFromIdValidationList(id);
        }
    }

    public void CreateUnvalidatedID(string id)
    {
        if (!string.IsNullOrWhiteSpace(id) && !DoesIDExist(id))
        {
            CreateNewId(id);
            AddToUnvalidatedIdList(id);
        }
    }

    public void ConfigureID(string id, Area area)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            SetPosition(
                id,
                area.Page!.getBody().getXPosition() + area.getTableCellXOffset() - IdPagging,
                area.Page!.getBody().GetYPosition() - area.getAbsoluteHeight() + IdPagging);

            SetPageNumber(id, area.Page?.getNumber() ?? 0);
            area.Page?.addToIDList(id);
        }
    }

    public string GetInvalidIds()
    {
        StringBuilder stringBuilder = new();
        foreach (object o in _idValidation.Keys)
        {
            stringBuilder.Append("\n\"");
            stringBuilder.Append(o.ToString());
            stringBuilder.Append("\" ");
        }

        return stringBuilder.ToString();
    }

    public bool DoesGoToReferenceExist(string id)
    {
        var node = (IDNode?)_idReferences[id];
        return node?.IsThereInternalLinkGoTo() ?? false;
    }

    public PdfGoTo? GetInternalLinkGoTo(string id)
    {
        var node = (IDNode?)_idReferences[id];
        return node?.GetInternalLinkGoTo();
    }

    public PdfGoTo? CreateInternalLinkGoTo(string id, PdfObjectId objectId)
    {
        var node = (IDNode?)_idReferences[id];
        node?.CreateInternalLinkGoTo(objectId);
        return node?.GetInternalLinkGoTo();
    }

    public void CreateNewId(string id)
    {
        var node = new IDNode(id);
        _idReferences[id] = node;
    }

    public PdfGoTo? GetPDFGoTo(string id)
    {
        var node = (IDNode?)_idReferences[id];
        return node?.GetInternalLinkGoTo();
    }

    public void SetInternalGoToPageReference(string id, PdfObjectReference pageReference)
    {
        var node = (IDNode?)_idReferences[id];
        node?.SetInternalLinkGoToPageReference(pageReference);
    }

    public void SetPageNumber(string id, int pageNumber)
    {
        var node = (IDNode?)_idReferences[id];
        node?.SetPageNumber(pageNumber);
    }

    public string? SetPageNumber(string id)
    {
        if (DoesIDExist(id))
        {
            var node = (IDNode?)_idReferences[id];
            return node?.GetPageNumber();
        }
        else
        {
            AddToIdValidationList(id);
            return null;
        }
    }

    public void SetPosition(string id, int x, int y)
    {
        var node = (IDNode?)_idReferences[id];
        node?.SetPosition(x, y);
    }
}