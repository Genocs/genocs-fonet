namespace Genocs.Fonet.DataTypes;

using Genocs.Fonet.Pdf;

internal class IDNode
{
    private readonly string _idValue;

    private PdfObjectReference? _internalLinkGoToPageReference;
    private PdfGoTo? _internalLinkGoTo;

    private int pageNumber = -1;
    private int xPosition = 0;
    private int yPosition = 0;

    internal IDNode(string idValue)
    {
        _idValue = idValue;
    }

    internal void SetPageNumber(int number)
    {
        pageNumber = number;
    }

    public string? GetPageNumber()
        => (pageNumber != -1) ? pageNumber.ToString() : null;

    internal void CreateInternalLinkGoTo(PdfObjectId objectId)
    {
        if (_internalLinkGoToPageReference == null)
        {
            _internalLinkGoTo = new PdfGoTo(null, objectId);
        }
        else
        {
            _internalLinkGoTo = new PdfGoTo(_internalLinkGoToPageReference, objectId);
        }

        if (xPosition != 0)
        {
            _internalLinkGoTo.X = xPosition;
            _internalLinkGoTo.Y = yPosition;
        }
    }

    internal void SetInternalLinkGoToPageReference(PdfObjectReference pageReference)
    {
        if (_internalLinkGoTo != null)
        {
            _internalLinkGoTo.PageReference = pageReference;
        }
        else
        {
            _internalLinkGoToPageReference = pageReference;
        }
    }

    internal string GetInternalLinkGoToReference()
        => _internalLinkGoTo != null ? $"{_internalLinkGoTo.ObjectId.ObjectNumber} {_internalLinkGoTo.ObjectId.GenerationNumber} R" : string.Empty;

    protected string GetIDValue()
        => _idValue;

    internal PdfGoTo? GetInternalLinkGoTo()
        => _internalLinkGoTo;

    internal bool IsThereInternalLinkGoTo()
        => _internalLinkGoTo != null;

    internal void SetPosition(int x, int y)
    {
        if (_internalLinkGoTo != null)
        {
            _internalLinkGoTo.X = x;
            _internalLinkGoTo.Y = y;
        }
        else
        {
            xPosition = x;
            yPosition = y;
        }
    }
}