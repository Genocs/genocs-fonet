using Genocs.Fonet.Pdf;
using Genocs.Fonet.Pdf.Filter;

namespace Genocs.Fonet.Pdf;

public class PdfFontFile : PdfStream
{
    public PdfFontFile(PdfObjectId id, byte[] fontData)
        : base(fontData, id)
    {
        this.AddFilter(new FlateFilter());
        this.dictionary[PdfName.Names.Length1] = new PdfNumeric(fontData.Length);
    }
}