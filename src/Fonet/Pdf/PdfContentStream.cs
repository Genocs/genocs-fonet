using System.Text;

namespace Genocs.Fonet.Pdf;

public class PdfContentStream : PdfStream
{
    private static readonly Encoding ContentStreamEncoding = Encoding.ASCII;

    protected readonly MemoryStream _stream;
    protected readonly PdfWriter _streamData;

    public PdfContentStream(PdfObjectId objectId)
        : base(objectId)
    {
        _stream = new MemoryStream();
        _streamData = new PdfWriter(_stream);
    }

    public void Write(PdfObject obj)
    {
        if (obj.IsIndirect || obj is PdfObjectReference)
        {
            throw new ArgumentException("Cannot write indirect PdfObject", "obj");
        }

        _streamData.Write(obj);
    }

    public void WriteLine(PdfObject obj)
    {
        if (obj.IsIndirect || obj is PdfObjectReference)
        {
            throw new ArgumentException("Cannot write indirect PdfObject", "obj");
        }

        _streamData.WriteLine(obj);
    }

    /// <summary>
    /// TODO: This method is temporary. I'm assuming that all string should 
    /// be represented as a PdfString object?
    /// </summary>
    /// <param name="s"></param>
    [Obsolete("Use Write(PdfString) instead")]
    public void Write(string s)
        => _streamData.Write(ContentStreamEncoding.GetBytes(s));

    public void WriteLine(string s)
        => _streamData.WriteLine(ContentStreamEncoding.GetBytes(s));

    public void Write(int val)
        => _streamData.Write(val);

    public void WriteLine(int val)
        => _streamData.WriteLine(val);

    public void Write(decimal val)
        => _streamData.Write(val);

    public void WriteLine(decimal val)
        => _streamData.WriteLine(val);

    public void WriteSpace()
        => _streamData.WriteSpace();

    public void WriteLine()
        => _streamData.WriteLine();

    public void WriteByte(byte value)
        => _streamData.WriteByte(value);

    public void Write(byte[] data)
        => _streamData.Write(data);

    public void WriteKeyword(Keyword keyword)
        => _streamData.WriteKeyword(keyword);

    public void WriteLine(byte[] data)
        => _streamData.WriteLine(data);

    protected internal override void Write(PdfWriter writer)
    {
        _data = _stream.ToArray();
        base.Write(writer);
    }
}