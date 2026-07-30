using System.Collections;

namespace Genocs.Fonet.Pdf;

public class PdfArray : PdfObject, IEnumerable
{
    private readonly ArrayList _elements = [];

    public PdfArray()
    {
    }

    public PdfArray(PdfObjectId objectId) : base(objectId)
    {
    }

    public int Add(PdfObject value)
        =>_elements.Add(value);

    public void Clear()
        => _elements.Clear();

    public bool Contains(PdfObject value)
        => _elements.Contains(value);

    public int IndexOf(PdfObject value)
        => _elements.IndexOf(value);

    public void Insert(int index, PdfObject value)
        => _elements.Insert(index, value);

    public void Remove(PdfObject value)
        => _elements.Remove(value);

    public void RemoveAt(int index)
        => _elements.RemoveAt(index);

    public IEnumerator GetEnumerator()
        => _elements.GetEnumerator();

    public PdfObject this[int index]
    {
        get
        {
            return (PdfObject)_elements[index];
        }
        set
        {
            _elements[index] = value;
        }
    }

    public int Count
    {
        get
        {
            return _elements.Count;
        }
    }

    public void AddArray(Array data)
    {
        foreach (object entry in data)
        {
            Add(new PdfNumeric(Convert.ToDecimal(entry)));
        }
    }

    protected internal override void Write(PdfWriter writer)
    {
        writer.WriteKeyword(Keyword.ArrayBegin);
        bool isFirst = true;
        foreach (PdfObject obj in _elements)
        {
            if (!isFirst)
            {
                writer.WriteSpace();
            }
            else
            {
                isFirst = false;
            }
            writer.Write(obj);
        }
        writer.WriteKeyword(Keyword.ArrayEnd);
    }
}