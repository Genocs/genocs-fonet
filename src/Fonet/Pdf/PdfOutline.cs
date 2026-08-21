using System.Collections;

namespace Genocs.Fonet.Pdf;

/// <summary>
/// This represents a single Outline object in a PDF, including the root Outlines
/// object. Outlines provide the bookmark bar, usually rendered to the right of
/// a PDF document in user agents such as Acrobat Reader
/// </summary>
public class PdfOutline : PdfObject
{
    /// <summary>
    /// List of sub-entries (outline objects)
    /// </summary>
    private ArrayList _subentries;

    /// <summary>
    /// Parent outline object. Root Outlines parent is null
    /// </summary>
    private PdfOutline? Parent;

    private PdfOutline? Previous;
    private PdfOutline? Next;

    private PdfOutline? First;
    private PdfOutline? Last;

    private int count;

    /// <summary>
    /// Title to display for the bookmark entry
    /// </summary>
    private string? _title;

    private PdfObjectReference? _actionRef;

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <param name="objectId">The object id number</param>
    /// <param name="title">The title of the outline entry (can only be null for root Outlines obj)</param>
    /// <param name="action">The page which this outline refers to.</param>
    public PdfOutline(PdfObjectId objectId, string? title, PdfObjectReference? action)
        : base(objectId)
    {
        _subentries = [];
        count = 0;
        Parent = null;
        Previous = null;
        Next = null;
        First = null;
        Last = null;
        _title = title;
        _actionRef = action;
    }

    public void SetTitle(string title)
        => _title = title;

    /// <summary>
    /// Add a sub element to this outline
    /// </summary>
    /// <param name="outline"></param>
    public void AddOutline(PdfOutline outline)
    {
        if (_subentries.Count > 0)
        {
            outline.Previous = (PdfOutline)_subentries[_subentries.Count - 1];
            outline.Previous.Next = outline;
        }
        else
        {
            First = outline;
        }

        _subentries.Add(outline);
        outline.Parent = this;

        IncrementCount(); // note: count is not just the immediate children

        Last = outline;
    }

    private void IncrementCount()
    {
        // count is a total of our immediate subentries and all descendent subentries
        count++;
        Parent?.IncrementCount();
    }

    protected internal override void Write(PdfWriter writer)
    {
        PdfDictionary dict = [];

        if (Parent == null)
        {
            // root Outlines object
            if (First != null && Last != null)
            {
                dict.Add(PdfName.Names.First, First.GetReference());
                dict.Add(PdfName.Names.Last, Last.GetReference());
            }

        }
        else
        {
            dict.Add(PdfName.Names.Title, new PdfString(_title));
            dict.Add(PdfName.Names.Parent, Parent.GetReference());

            if (First != null && Last != null)
            {
                dict.Add(PdfName.Names.First, First.GetReference());
                dict.Add(PdfName.Names.Last, Last.GetReference());
            }
            if (Previous != null)
            {
                dict.Add(PdfName.Names.Prev, Previous.GetReference());
            }
            if (Next != null)
            {
                dict.Add(PdfName.Names.Next, Next.GetReference());
            }
            if (count > 0)
            {
                dict.Add(PdfName.Names.Count, new PdfNumeric(count));
            }

            if (_actionRef != null)
            {
                dict.Add(PdfName.Names.A, _actionRef);
            }
        }

        writer.Write(dict);
    }
}