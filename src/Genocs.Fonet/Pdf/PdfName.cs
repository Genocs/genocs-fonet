using System.Text;

namespace Genocs.Fonet.Pdf;

public sealed class PdfName : PdfObject
{
    private byte[]? bytes;
    public string Name { get; }

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
            if (bytes == null)
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
                bytes = ms.ToArray();
            }
            return bytes;
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

    //        public static bool operator ==(PdfName o1, PdfName o2) {
    //            return o1.Equals(o2);
    //        }
    //
    //        public static bool operator !=(PdfName o1, PdfName o2) {
    //            return !(o1 == o2);
    //        }

    /// <summary>
    /// Well-known PDF name objects.
    /// </summary>
    public class Names
    {
        public static readonly PdfName Catalog = new("Catalog");
        public static readonly PdfName Type = new("Type");
        public static readonly PdfName Subtype = new("Subtype");
        public static readonly PdfName Pages = new("Pages");
        public static readonly PdfName Outlines = new("Outlines");
        public static readonly PdfName Kids = new("Kids");
        public static readonly PdfName Count = new("Count");

        public static readonly PdfName Title = new("Title");
        public static readonly PdfName Author = new("Author");
        public static readonly PdfName Subject = new("Subject");
        public static readonly PdfName Keywords = new("Keywords");
        public static readonly PdfName Creator = new("Creator");
        public static readonly PdfName Producer = new("Producer");
        public static readonly PdfName CreationDate = new("CreationDate");
        public static readonly PdfName ModDate = new("ModDate");

        public static readonly PdfName Size = new("Size");
        public static readonly PdfName Prev = new("Prev");
        public static readonly PdfName Root = new("Root");
        public static readonly PdfName Encrypt = new("Encrypt");
        public static readonly PdfName Info = new("Info");
        public static readonly PdfName Id = new("ID");

        public static readonly PdfName Encoding = new("Encoding");
        public static readonly PdfName BaseEncoding = new("BaseEncoding");
        public static readonly PdfName MacRomanEncoding = new("MacRomanEncoding");
        public static readonly PdfName MacExpertEncoding = new("MacExpertEncoding");
        public static readonly PdfName WinAnsiEncoding = new("WinAnsiEncoding");

        public static readonly PdfName FileSpec = new("FileSpec");
        public static readonly PdfName F = new("F");

        public static readonly PdfName Annot = new("Annot");
        public static readonly PdfName Action = new("Action");
        public static readonly PdfName Link = new("Link");
        public static readonly PdfName H = new("H");
        public static readonly PdfName I = new("I");
        public static readonly PdfName A = new("A");
        public static readonly PdfName Border = new("Border");
        public static readonly PdfName Rect = new("Rect");
        public static readonly PdfName C = new("C");
        public static readonly PdfName S = new("S");
        public static readonly PdfName GoTo = new("GoTo");
        public static readonly PdfName GoToR = new("GoToR");
        public static readonly PdfName D = new("D");
        public static readonly PdfName XYZ = new("XYZ");
        public static readonly PdfName URI = new("URI");

        public static readonly PdfName Font = new("Font");
        public static readonly PdfName FontName = new("FontName");
        public static readonly PdfName FontDescriptor = new("FontDescriptor");
        public static readonly PdfName Flags = new("Flags");
        public static readonly PdfName FontBBox = new("FontBBox");
        public static readonly PdfName ItalicAngle = new("ItalicAngle");
        public static readonly PdfName Ascent = new("Ascent");
        public static readonly PdfName Descent = new("Descent");
        public static readonly PdfName Leading = new("Leading");
        public static readonly PdfName CapHeight = new("CapHeight");
        public static readonly PdfName XHeight = new("XHeight");
        public static readonly PdfName StemV = new("StemV");
        public static readonly PdfName StemH = new("StemH");
        public static readonly PdfName AvgWidth = new("AvgWidth");
        public static readonly PdfName MaxWidth = new("MaxWidth");
        public static readonly PdfName MissingWidth = new("MissingWidth");
        public static readonly PdfName FontFile = new("FontFile");
        public static readonly PdfName FontFile2 = new("FontFile2");
        public static readonly PdfName FontFile3 = new("FontFile3");
        public static readonly PdfName CharSet = new("CharSet");
        public static readonly PdfName CIDToGIDMap = new("CIDToGIDMap");
        public static readonly PdfName Identity = new("Identity");

        public static readonly PdfName Length1 = new("Length1");
        public static readonly PdfName Length2 = new("Length2");
        public static readonly PdfName Length3 = new("Length3");

        public static readonly PdfName ToUnicode = new("ToUnicode");
        public static readonly PdfName CMap = new("CMap");
        public static readonly PdfName CMapName = new("CMapName");
        public static readonly PdfName WMode = new("WMode");

        public static readonly PdfName Type0 = new("Type0");
        public static readonly PdfName Type1 = new("Type1");
        public static readonly PdfName TrueType = new("TrueType");
        public static readonly PdfName Name = new("Name");
        public static readonly PdfName BaseFont = new("BaseFont");
        public static readonly PdfName XObject = new("XObject");

        public static readonly PdfName CIDFontType0 = new("CIDFontType0");
        public static readonly PdfName CIDFontType2 = new("CIDFontType2");
        public static readonly PdfName CIDSystemInfo = new("CIDSystemInfo");
        public static readonly PdfName DescendantFonts = new("DescendantFonts");

        public static readonly PdfName Registry = new("Registry");
        public static readonly PdfName Ordering = new("Ordering");
        public static readonly PdfName Supplement = new("Supplement");

        public static readonly PdfName DW = new("DW");
        public static readonly PdfName W = new("W");

        public static readonly PdfName Page = new("Page");
        public static readonly PdfName PageMode = new("PageMode");
        public static readonly PdfName UseOutlines = new("UseOutlines");
        public static readonly PdfName Resources = new("Resources");
        public static readonly PdfName Contents = new("Contents");
        public static readonly PdfName MediaBox = new("MediaBox");
        public static readonly PdfName Parent = new("Parent");
        public static readonly PdfName Annots = new("Annots");

        public static readonly PdfName Image = new("Image");
        public static readonly PdfName Width = new("Width");
        public static readonly PdfName Height = new("Height");
        public static readonly PdfName BitsPerComponent = new("BitsPerComponent");
        public static readonly PdfName ColorSpace = new("ColorSpace");

        public static readonly PdfName ProcSet = new("ProcSet");
        public static readonly PdfName PDF = new("PDF");
        public static readonly PdfName Text = new("Text");
        public static readonly PdfName ImageB = new("ImageB");
        public static readonly PdfName ImageC = new("ImageC");
        public static readonly PdfName ImageI = new("ImageI");

        public static readonly PdfName Length = new("Length");
        public static readonly PdfName Filter = new("Filter");
        public static readonly PdfName DecodeParams = new("DecodeParams");

        public static readonly PdfName ASCII85Decode = new("ASCII85Decode");
        public static readonly PdfName ASCIIHexDecode = new("ASCIIHexDecode");
        public static readonly PdfName CCITTFaxDecode = new("CCITTFaxDecode");
        public static readonly PdfName DCTDecode = new("DCTDecode");
        public static readonly PdfName FlateDecode = new("FlateDecode");
        public static readonly PdfName JBIG2Decode = new("JBIG2Decode");
        public static readonly PdfName LZWDecode = new("LZWDecode");
        public static readonly PdfName RunLengthDecode = new("RunLengthDecode");

        public static readonly PdfName Standard = new("Standard");
        public static readonly PdfName V = new("V");
        public static readonly PdfName R = new("R");
        public static readonly PdfName O = new("O");
        public static readonly PdfName U = new("U");
        public static readonly PdfName P = new("P");

        public static readonly PdfName FirstChar = new("FirstChar");
        public static readonly PdfName LastChar = new("LastChar");
        public static readonly PdfName Widths = new("Widths");

        public static readonly PdfName First = new("First");
        public static readonly PdfName Last = new("Last");
        public static readonly PdfName Next = new("Next");

        public static readonly PdfName Alternate = new("Alternate");
        public static readonly PdfName ICCBased = new("ICCBased");
        public static readonly PdfName N = new("N");
    }
}