using System.Text;

namespace Genocs.Fonet.Pdf;

public sealed partial class PdfName : PdfObject
{
    public string Name { get; }

    private byte[]? _bytes;

    public PdfName(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public PdfName(string name, PdfObjectId objectId)
        : base(objectId)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    protected internal override void Write(PdfWriter writer)
    {
        writer.Write(NameBytes);
    }

    private static readonly byte[] HexDigits = [
        0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
        0x38, 0x39, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66
    ];

    private byte[] NameBytes
    {
        get
        {
            if (_bytes == null)
            {
                // Create a memory stream to hold the results.
                // We guess the size, based on the most likely outcome
                // (i.e. all ASCII _characters with no escapes.
                MemoryStream ms = new(Name.Length + 1);

                // The forward slash introduces a name.
                ms.WriteByte((byte)'/');

                // The PDF specification recommends encoding name objects using UTF8.
                byte[] data = Encoding.UTF8.GetBytes(Name);
                for (int x = 0; x < data.Length; x++)
                {
                    byte b = data[x];

                    // The PDF specification recommends using a special #hh syntax 
                    // for any bytes that are outside the range 33 to 126 and for
                    // the # character itself (35).
                    if (b < 34 || b > 125 || b == 35)
                    {
                        ms.WriteByte((byte)'#');
                        ms.WriteByte(HexDigits[b >> 4]);
                        ms.WriteByte(HexDigits[b & 0x0f]);
                    }
                    else
                    {
                        ms.WriteByte(b);
                    }
                }

                ms.Close();
                _bytes = ms.ToArray();
            }

            return _bytes;
        }
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is not PdfName pobj)
        {
            return false;
        }

        return Name.Equals(pobj.Name);
    }
}