using Genocs.Fonet.Pdf.Security;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Genocs.Fonet.Pdf;

public class PdfWriter
{
    public static readonly byte[] DefaultNewLine = [0x0d, 0x0a];

    public static readonly byte[] DefaultSpace = [0x20];

    public static readonly byte[] DefaultBinaryComment = [0x25, 0xe2, 0xe3, 0xcf, 0xd3];

    private readonly Stream _stream;

    private readonly Stack _indirectObjects = new();

    private readonly byte[] _space = DefaultSpace;
    public  byte[] NewLine { get; private set; } = DefaultNewLine;
    private byte[] BinaryComment { get; set; } = DefaultBinaryComment;
    public long Position { get; private set; }
    public SecurityManager SecurityManager { get; set; }

    public PdfWriter(Stream stream)
    {
        Debug.Assert(stream.CanWrite);
        _stream = stream;
    }

    internal PdfObject EnclosingIndirect
    {
        get
        {
            Debug.Assert(_indirectObjects.Count > 0);
            return (PdfObject)_indirectObjects.Peek();
        }
    }

    public void Close()
        => _stream.Close();

    public void WriteHeader(PdfVersion version)
        => WriteLine(version.Header);

    public void WriteBinaryComment()
        => WriteLine(BinaryComment);

    public void Write(PdfObject? obj)
    {
        Debug.Assert(obj != null);
        if (obj.IsIndirect)
        {
            _indirectObjects.Push(obj);
            obj.WriteIndirect(this);
            _indirectObjects.Pop();
        }
        else
        {
            obj.Write(this);
        }
    }

    public void WriteLine(PdfObject? obj)
    {
        Debug.Assert(obj != null);
        Write(obj);
        WriteLine();
    }

    public void Write(int val)
    {
        byte[] data = Encoding.ASCII.GetBytes(val.ToString());
        Write(data);
    }

    public void WriteLine(int val)
    {
        Write(val);
        WriteLine();
    }

    public void Write(decimal val)
    {
        // TODO: This conversion could produce a number expressed
        // in scientific format which is not supported by PDF.
        Debug.Assert(val.ToString().IndexOfAny(['e', 'E']) == -1);

        // The invariant culture will ensure a dot ('.') is used as the 
        // decimal point. The French culture, for example, uses a comma.
        byte[] data = Encoding.ASCII.GetBytes(val.ToString(CultureInfo.InvariantCulture));
        Write(data);
    }

    public void WriteLine(decimal val)
    {
        Write(val);
        WriteLine();
    }

    public void WriteSpace()
    {
        _stream.Write(_space, 0, _space.Length);
        Position += _space.Length;
    }

    public void WriteLine()
    {
        _stream.Write(NewLine, 0, NewLine.Length);
        Position += NewLine.Length;
    }

    public void WriteByte(byte value)
    {
        _stream.WriteByte(value);
        Position++;
    }

    public void Write(byte[]? data)
    {
        if (data != null)
        {
            _stream.Write(data, 0, data.Length);
            Position += data.Length;
        }
    }

    public void WriteLine(byte[]? data)
    {
        Write(data);
        WriteLine();
    }

    public void WriteKeyword(Keyword keyword)
        => Write(KeywordEntries.GetKeyword(keyword));

    public void WriteKeywordLine(Keyword keyword)
    {
        WriteKeyword(keyword);
        WriteLine();
    }
}