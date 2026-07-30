using System.Collections;

namespace Genocs.Fonet.Pdf;

public class PdfDictionary : PdfObject, IEnumerable
{
    protected Hashtable _entries = [];

    public PdfDictionary()
    {
    }

    public PdfDictionary(PdfObjectId objectId)
        : base(objectId)
    {
    }

    public void Add(PdfName key, PdfObject value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_entries.ContainsKey(key))
        {
            throw new ArgumentException($"Already contains entry {key}");
        }

        _entries.Add(key, value);
    }

    public void Clear()
    {
        _entries.Clear();
    }

    public bool Contains(PdfName key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return _entries.ContainsKey(key);
    }

    public void Remove(PdfName key)
    {
        ArgumentNullException.ThrowIfNull(key);
        _entries.Remove(key);
    }

    public IEnumerator GetEnumerator()
    {
        return _entries.GetEnumerator();
    }

    public PdfObject this[PdfName key]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);
            return (PdfObject)_entries[key];
        }
        set
        {
            ArgumentNullException.ThrowIfNull(key);
            _entries[key] = value;
        }
    }

    public ICollection Keys
    {
        get
        {
            return _entries.Keys;
        }
    }

    public ICollection Values
    {
        get
        {
            return _entries.Values;
        }
    }

    public int Count
    {
        get
        {
            return _entries.Count;
        }
    }

    protected internal override void Write(PdfWriter writer)
    {
        writer.WriteKeywordLine(Keyword.DictionaryBegin);
        foreach (DictionaryEntry e in _entries)
        {
            writer.Write((PdfName)e.Key);
            writer.WriteSpace();
            writer.WriteLine((PdfObject?)e.Value);
        }
        writer.WriteKeyword(Keyword.DictionaryEnd);
    }

}