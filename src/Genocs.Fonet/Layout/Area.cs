using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo;
using Genocs.Fonet.Fo.Flow;
using Genocs.Fonet.Layout.Inline;
using Genocs.Fonet.Render.Pdf;
using System.Collections;
using System.Collections.Generic;

namespace Genocs.Fonet.Layout;

internal abstract class Area : Box
{
    protected BorderAndPadding? bp;

    protected int maxHeight;
    protected int currentHeight = 0;
    protected int tableCellXOffset = 0;
    private int absoluteYTop = 0;
    protected int contentRectangleWidth;
    protected int allocationWidth;
    protected BackgroundProps background;
    private IDReferences idReferences;
    protected Hashtable returnedBy = new();
    protected string? areaClass;
    public FObj foCreator;
    public virtual Page? Page { get; set; }


    /// <summary>
    /// Gets the list of child areas contained within this area.
    /// </summary>
    public ArrayList Children { get; } = [];

    /// <summary>
    /// Gets the list of markers associated with this area.
    /// </summary>
    protected ArrayList Markers { get; } = [];

    public FontState? FontState { get; set; }
    public FObj GeneratedBy { get; init; }
    public bool IsFirst { get; set; }
    public bool IsLast { get; set; }
    public int ZIndex { get; set; }

    protected Area(FontState? fontState, Area? parent) : base(parent)
    {
        FontState = fontState;
    }

    protected Area(FontState? fontState, int allocationWidth, int maxHeight, Area? parent) : this(fontState, parent)
    {
        this.allocationWidth = allocationWidth;
        this.contentRectangleWidth = allocationWidth;
        this.maxHeight = maxHeight;
    }

    public void AddChild(Box child)
    {
        Children.Add(child);
        //child.ForceParent(this);
    }

    public void AddChildAtStart(Box child)
    {
        Children.Insert(0, child);
        child.ForceParent(this);
    }

    public void AddDisplaySpace(int size)
    {
        AddChild(new DisplaySpace(size));
        this.currentHeight += size;
    }

    public void AddInlineSpace(int size)
        => AddChild(new InlineSpace(size));

    public FontInfo? GetFontInfo()
        => Page?.GetFontInfo();

    public virtual void end()
    {
    }

    public int getAllocationWidth()
    {
        return this.allocationWidth;
    }

    public void setAllocationWidth(int w)
    {
        this.allocationWidth = w;
        this.contentRectangleWidth = this.allocationWidth;
    }

    public bool hasChildren()
    {
        return (Children.Count != 0);
    }

    public bool HasNonSpaceChildren()
    {
        if (Children.Count > 0)
        {
            foreach (object child in Children)
            {
                if (child is not DisplaySpace)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public virtual int getContentWidth()
    {
        return contentRectangleWidth;
    }

    public virtual int getContentHeight()
    {
        return this.currentHeight;
    }

    public virtual int GetHeight()
    {
        return this.currentHeight + getPaddingTop() + getPaddingBottom()
            + getBorderTopWidth() + getBorderBottomWidth();
    }

    public int getMaxHeight()
    {
        return this.maxHeight;
    }

    public BackgroundProps getBackground()
    {
        return this.background;
    }

    public int getPaddingTop()
    {
        return (bp == null ? 0 : bp.GetPaddingTop(false));
    }

    public int getPaddingLeft()
    {
        return (bp == null ? 0 : bp.GetPaddingLeft(false));
    }

    public int getPaddingBottom()
    {
        return (bp == null ? 0 : bp.GetPaddingBottom(false));
    }

    public int getPaddingRight()
    {
        return (bp == null ? 0 : bp.GetPaddingRight(false));
    }

    public int getBorderTopWidth()
    {
        return (bp == null ? 0 : bp.GetBorderTopWidth(false));
    }

    public int getBorderRightWidth()
    {
        return (bp == null ? 0 : bp.GetBorderRightWidth(false));
    }

    public int getBorderLeftWidth()
    {
        return (bp == null ? 0 : bp.GetBorderLeftWidth(false));
    }

    public int getBorderBottomWidth()
    {
        return (bp == null ? 0 : bp.GetBorderBottomWidth(false));
    }

    public int getTableCellXOffset()
    {
        return tableCellXOffset;
    }

    public void setTableCellXOffset(int offset)
    {
        tableCellXOffset = offset;
    }

    public int getAbsoluteHeight()
    {
        return absoluteYTop + getPaddingTop() + getBorderTopWidth() + currentHeight;
    }

    public void setAbsoluteHeight(int value)
    {
        absoluteYTop = value;
    }

    public void increaseHeight(int amount)
    {
        this.currentHeight += amount;
    }

    public void removeChild(Area area)
    {
        this.currentHeight -= area.GetHeight();
        this.Children.Remove(area);
    }

    public void removeChild(DisplaySpace spacer)
    {
        this.currentHeight -= spacer.getSize();
        this.Children.Remove(spacer);
    }

    public void remove()
        => Parent?.removeChild(this);

    public void setBackground(BackgroundProps bg)
    {
        this.background = bg;
    }

    public void setBorderAndPadding(BorderAndPadding bp)
    {
        this.bp = bp;
    }

    public virtual int spaceLeft()
    {
        return maxHeight - currentHeight;
    }

    public virtual void start()
    {
    }

    public virtual void SetHeight(int height)
    {
        int prevHeight = currentHeight;
        if (height > currentHeight)
        {
            currentHeight = height;
        }

        if (currentHeight > getMaxHeight())
        {
            currentHeight = getMaxHeight();
        }
    }

    public void setMaxHeight(int height)
    {
        this.maxHeight = height;
    }

    public virtual void setIDReferences(IDReferences idReferences)
    {
        this.idReferences = idReferences;
    }

    public virtual IDReferences GetIDReferences()
    {
        return idReferences;
    }

    public FObj getfoCreator()
    {
        return this.foCreator;
    }

    public AreaContainer? getNearestAncestorAreaContainer()
    {
        Area? area = Parent;
        while (area != null && area is not AreaContainer)
        {
            area = area.Parent;
        }

        return (AreaContainer?)area;
    }

    public BorderAndPadding GetBorderAndPadding()
    {
        return bp;
    }

    public void addMarker(Marker marker)
    {
        Markers.Add(marker);
    }

    public void addMarkers(ArrayList markers)
    {
        foreach (object o in markers)
        {
            this.Markers.Add(o);
        }
    }

    public void addLineagePair(FObj fo, int areaPosition)
    {
        returnedBy.Add(fo, areaPosition);
    }

    public ArrayList GetMarkers()
    {
        return Markers;
    }

    internal static void RenderChildrenInZOrder(ArrayList children, PdfRenderer renderer)
    {
        foreach (Box child in GetChildrenInZOrder(children))
        {
            child.Render(renderer);
        }
    }

    internal static List<Box> GetChildrenInZOrder(ArrayList children)
    {
        if (children.Count <= 1)
        {
            var single = new List<Box>(children.Count);
            foreach (Box child in children)
            {
                single.Add(child);
            }

            return single;
        }

        var indexed = new List<(int Index, Box Box, int ZIndex)>(children.Count);
        for (int i = 0; i < children.Count; i++)
        {
            Box box = (Box)children[i]!;
            int zIndex = box is Area area ? area.ZIndex : 0;
            indexed.Add((i, box, zIndex));
        }

        indexed.Sort(static (left, right) =>
        {
            int byZIndex = left.ZIndex.CompareTo(right.ZIndex);
            return byZIndex != 0 ? byZIndex : left.Index.CompareTo(right.Index);
        });

        var ordered = new List<Box>(indexed.Count);
        foreach (var entry in indexed)
        {
            ordered.Add(entry.Box);
        }

        return ordered;
    }
}