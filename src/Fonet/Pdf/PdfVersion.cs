using System.Text;

namespace Genocs.Fonet.Pdf;

public sealed class PdfVersion
{
    public byte Major { get; }
    public byte Minor { get; }

    private PdfVersion(byte major, byte minor)
    {
        Major = major;
        Minor = minor;
    }

    public byte[] Header
    {
        get
        {
            return Encoding.ASCII.GetBytes($"%PDF-{Major}.{Minor}");
        }
    }

    public static readonly PdfVersion V10 = new(1, 0);
    public static readonly PdfVersion V11 = new(1, 1);
    public static readonly PdfVersion V12 = new(1, 2);
    public static readonly PdfVersion V13 = new(1, 3);
    public static readonly PdfVersion V14 = new(1, 4);
    public static readonly PdfVersion V15 = new(1, 5);
    public static readonly PdfVersion V16 = new(1, 6);
    public static readonly PdfVersion V17 = new(1, 7);
    public static readonly PdfVersion V20 = new(2, 0);
}