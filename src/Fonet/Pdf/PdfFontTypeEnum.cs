namespace Genocs.Fonet.Pdf
{
    /// <summary>
    /// An enumeration listing all the fonts types available in Pdf.
    /// </summary>
    public enum PdfFontTypeEnum
    {
        /// <summary>
        /// A composite font
        /// </summary>
        Type0,

        /// <summary>
        /// Adobe font
        /// </summary>
        Type1,

        /// <summary>
        /// Font whose glyphs are defined by Adobe graphic operators
        /// </summary>
        Type3,

        /// <summary>
        /// Font based on TrueType format
        /// </summary>
        TrueType,

        /// <summary>
        /// Font-like object whose glyph descriptions are defined in a descendant font
        /// </summary>
        CIDFont
    }

    /// <summary>
    /// An enumeration listing all the font subtypes
    /// </summary>
    public enum PdfFontSubTypeEnum
    {
        Type0,
        Type1,
        MMType1,
        Type3,
        TrueType,
        CIDFontType0,
        CIDFontType2
    }
}