using Genocs.Fonet.Pdf.Filter;
using Genocs.Fonet.Pdf.Security;

namespace Genocs.Fonet.Pdf;

public class PdfStream : PdfObject
{
    protected byte[]? _data;
    protected PdfDictionary _dictionary = [];

    private List<IFilter>? _filters;

    public PdfStream()
    {
    }

    public PdfStream(PdfObjectId objectId)
        : base(objectId)
    {
    }

    public PdfStream(byte[] data)
    {
        _data = data;
    }

    public PdfStream(byte[] data, PdfObjectId objectId)
        : base(objectId)
    {
        _data = data;
    }

    public void AddFilter(IFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        _filters ??= [];
        _filters.Add(filter);
    }

    private PdfObject FilterName
    {
        get
        {
            if (!HasFilters)
            {
                return PdfNull.Null;
            }
            else if (_filters.Count == 1)
            {
                IFilter filter = _filters[0];
                return filter.Name;
            }
            else
            {
                PdfArray names = new PdfArray();
                foreach (IFilter filter in _filters)
                {
                    names.Add(filter.Name);
                }

                return names;
            }
        }
    }

    private PdfObject FilterDecodeParms
    {
        get
        {
            if (!HasFilters)
            {
                return PdfNull.Null;
            }
            else if (_filters.Count == 1)
            {
                IFilter filter = _filters[0];
                return filter.DecodeParms;
            }
            else
            {
                PdfArray decodeParams = new PdfArray();
                foreach (IFilter filter in _filters)
                {
                    decodeParams.Add(filter.DecodeParms);
                }

                return decodeParams;
            }
        }
    }

    private bool HasFilters
    {
        get
        {
            return _filters?.Count > 0;
        }
    }

    private bool HasDecodeParams
    {
        get
        {
            return _filters?.Any(c => c.HasDecodeParams) ?? false;
        }
    }

    private byte[] ApplyFilters(byte[] data)
    {
        if (_filters == null)
        {
            return data;
        }

        byte[] encoded = data;
        for (int x = _filters.Count - 1; x >= 0; x--)
        {
            IFilter filter = _filters[x];
            encoded = filter.Encode(encoded);
        }

        return encoded;
    }

    protected internal override void Write(PdfWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (_data == null)
        {
            throw new InvalidOperationException("No data for stream.");
        }

        // Prepare the stream's data.
        byte[] bytes = (byte[])_data.Clone();

        // Apply any filters.
        if (HasFilters)
        {
            bytes = ApplyFilters(_data);
        }

        // Encrypt the data if required.
        SecurityManager sm = writer.SecurityManager;
        if (sm != null)
        {
            bytes = sm.Encrypt(bytes, writer.EnclosingIndirect.ObjectId);
        }

        // Create the stream's dictionary.
        _dictionary[PdfName.Names.Length] = new PdfNumeric(bytes.Length);
        if (HasFilters)
        {
            _dictionary[PdfName.Names.Filter] = FilterName;
            if (HasDecodeParams)
            {
                _dictionary[PdfName.Names.DecodeParams] = FilterDecodeParms;
            }
        }

        // Write out the dictionary.
        writer.WriteLine(_dictionary);

        // Write out the stream data.
        writer.WriteKeywordLine(Keyword.Stream);
        writer.WriteLine(bytes);
        writer.WriteKeyword(Keyword.EndStream);
    }
}