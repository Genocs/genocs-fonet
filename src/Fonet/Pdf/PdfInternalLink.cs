namespace Genocs.Fonet.Pdf;

public class PdfInternalLink : IPdfAction
{
    private PdfObjectReference _goToReference;

    public PdfInternalLink(PdfObjectReference goToReference)
    {
        _goToReference = goToReference;
    }

    public PdfObject GetAction()
    {
        return _goToReference;
    }
}