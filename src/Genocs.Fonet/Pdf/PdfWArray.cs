namespace Genocs.Fonet.Pdf;

/// <summary>
/// Array class used to represent the /W entry in the CIDFont dictionary.
/// </summary>
public class PdfWArray(int StartCID) : PdfObject
{
    private readonly PdfArray _array = [];

    public void AddEntry(int[] widths)
    {
        _array.AddArray(widths);
    }

    protected internal override void Write(PdfWriter? writer)
    {
        writer!.WriteKeyword(Keyword.ArrayBegin);
        writer.WriteSpace();
        writer.Write(StartCID);
        writer.WriteSpace();
        _array.Write(writer);
        writer.WriteKeyword(Keyword.ArrayEnd);
    }
}