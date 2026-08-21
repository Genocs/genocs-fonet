using Genocs.Fonet.Pdf;

namespace Genocs.Fonet.Pdf
{
    public class PdfXObject : PdfStream
    {
        private byte[] objectData;

        private PdfName name;

        public PdfXObject(byte[] objectData, PdfName name, PdfObjectId objectId)
            : base(objectId)
        {
            this.objectData = objectData;
            this.name = name;
            _dictionary[PdfName.Names.Type] = PdfName.Names.XObject;
        }

        public PdfName SubType
        {
            get { return (PdfName)_dictionary[PdfName.Names.Subtype]; }
            set { _dictionary[PdfName.Names.Subtype] = value; }
        }

        public PdfName Name
        {
            get { return name; }
        }

        public PdfDictionary Dictionary
        {
            get { return _dictionary; }
        }

        protected internal override void Write(PdfWriter writer)
        {
            _data = objectData;
            base.Write(writer);
        }

    }
}