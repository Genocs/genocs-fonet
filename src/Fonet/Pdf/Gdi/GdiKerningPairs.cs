using Genocs.Fonet.Pdf.Gdi.Font;

namespace Genocs.Fonet.Pdf.Gdi;

public class GdiKerningPairs
{
    public static readonly GdiKerningPairs Empty = new(null, null);

    private readonly KerningPairs? _pairs;
    private readonly PdfUnitConverter? _converter;

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <param name="pairs">Kerning pairs read from the TrueType font file.</param>
    /// <param name="converter">Class to convert from TTF to PDF units.</param>
    internal GdiKerningPairs(KerningPairs? pairs, PdfUnitConverter? converter)
    {
        _pairs = pairs;
        _converter = converter;
    }

    /// <summary>
    /// Gets the number of kerning pairs.
    /// </summary>
    public int Count
    {
        get { return (_pairs == null) ? 0 : _pairs.Length; }
    }

    /// <summary>
    /// Returns true if a kerning value exists for the supplied 
    /// character index pair.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns>True if a kerning value exists for the supplied character index pair; otherwise, false.</returns>
    public bool HasPair(ushort left, ushort right)
    {
        return _pairs != null && _pairs.HasKerning(left, right);
    }

    /// <summary>
    /// Gets the kerning amount for the supplied index pair or 0 if 
    /// a kerning pair does not exist.
    /// </summary>
    public int this[ushort left, ushort right]
    {
        get
        {
            // TODO: Crapy performance
            if (_pairs == null || _converter == null)
            {
                return 0;
            }
            return _converter.ToPdfUnits(_pairs[left, right]);
        }
    }
}