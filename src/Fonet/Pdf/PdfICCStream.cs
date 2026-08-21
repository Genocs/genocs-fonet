using Genocs.Fonet.Pdf;

namespace Genocs.Fonet.Pdf
{
    /// <summary>
    /// An International Color Code stream
    /// </summary>
    public class PdfICCStream : PdfStream
    {
        public PdfICCStream(PdfObjectId id, byte[] profileData)
            : base(id)
        {
            _data = profileData;
        }

        public PdfNumeric NumComponents
        {
            set { this._dictionary[PdfName.Names.N] = value; }
        }

        public PdfString AlternativeColorSpace
        {
            set { this._dictionary[PdfName.Names.Alternate] = value; }
        }

    }
}