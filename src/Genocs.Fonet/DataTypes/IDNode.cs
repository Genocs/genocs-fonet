namespace Genocs.Fonet.DataTypes;

using Genocs.Fonet.Pdf;

internal class IDNode
{
    private readonly string _idValue;

    private PdfObjectReference? internalLinkGoToPageReference;
    private PdfGoTo? internalLinkGoTo;

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
    {
        return (pageNumber != -1) ? pageNumber.ToString() : null;
    }

    internal void CreateInternalLinkGoTo(PdfObjectId objectId)
    {
        if (internalLinkGoToPageReference == null)
        {
            internalLinkGoTo = new PdfGoTo(null, objectId);
        }
        else
        {
            internalLinkGoTo = new PdfGoTo(internalLinkGoToPageReference, objectId);
        }

        if (xPosition != 0)
        {
            internalLinkGoTo.X = xPosition;
            internalLinkGoTo.Y = yPosition;
        }
    }

    internal void SetInternalLinkGoToPageReference(PdfObjectReference pageReference)
    {
        if (internalLinkGoTo != null)
        {
            internalLinkGoTo.PageReference = pageReference;
        }
        else
        {
            internalLinkGoToPageReference = pageReference;
        }
    }

    internal string GetInternalLinkGoToReference()
        => internalLinkGoTo != null ? $"{internalLinkGoTo.ObjectId.ObjectNumber} {internalLinkGoTo.ObjectId.GenerationNumber} R" : string.Empty;

    protected string GetIDValue()
        => _idValue;

    internal PdfGoTo? GetInternalLinkGoTo()
        => internalLinkGoTo;

    internal bool IsThereInternalLinkGoTo()
        => internalLinkGoTo != null;

    internal void SetPosition(int x, int y)
    {
        if (internalLinkGoTo != null)
        {
            internalLinkGoTo.X = x;
            internalLinkGoTo.Y = y;
        }
        else
        {
            xPosition = x;
            yPosition = y;
        }
    }
}