using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo;
using Genocs.Fonet.Fo.Flow;
using Genocs.Fonet.Layout.Inline;
using System.Collections;

namespace Genocs.Fonet.Layout;

internal abstract class Area : Box
{
    public FontState? FontState { get; set; }

    protected BorderAndPadding bp = null;
    protected ArrayList _children = [];
    protected int maxHeight;
    protected int currentHeight = 0;
    protected int tableCellXOffset = 0;
    private int absoluteYTop = 0;
    protected int contentRectangleWidth;
    protected int allocationWidth;
    protected Page? page;
    protected BackgroundProps background;
    private IDReferences idReferences;
    protected ArrayList markers;
    public FObj GeneratedBy { get; set; }
    protected Hashtable returnedBy;
    protected string areaClass = null;

    public bool IsFirst { get; set; }
    public bool IsLast { get; set; }

    public FObj foCreator;

    public Area(FontState? fontState)
    {
        FontState = fontState;
        this.markers = new ArrayList();
        this.returnedBy = new Hashtable();
    }

    public Area(FontState? fontState, int allocationWidth, int maxHeight)
    {
        FontState = fontState;
        this.allocationWidth = allocationWidth;
        this.contentRectangleWidth = allocationWidth;
        this.maxHeight = maxHeight;
        this.markers = new ArrayList();
        this.returnedBy = new Hashtable();
    }

    public void addChild(Box child)
    {
        this._children.Add(child);
        child.parent = this;
    }

    public void addChildAtStart(Box child)
    {
        this._children.Insert(0, child);
        child.parent = this;
    }

    public void addDisplaySpace(int size)
    {
        this.addChild(new DisplaySpace(size));
        this.currentHeight += size;
    }

    public void addInlineSpace(int size)
    {
        this.addChild(new InlineSpace(size));
    }

    public FontInfo? getFontInfo()
        => page?.GetFontInfo();

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

    public ArrayList getChildren()
        => _children;

    public bool hasChildren()
    {
        return (_children.Count != 0);
    }

    public bool HasNonSpaceChildren()
    {
        if (_children.Count > 0)
        {
            foreach (object child in _children)
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

    public Page getPage()
    {
        return this.page;
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
        this._children.Remove(area);
    }

    public void removeChild(DisplaySpace spacer)
    {
        this.currentHeight -= spacer.getSize();
        this._children.Remove(spacer);
    }

    public void remove()
    {
        this.parent.removeChild(this);
    }

    public virtual void setPage(Page page)
    {
        this.page = page;
    }

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

    public Area getParent()
    {
        return this.parent;
    }

    public void setParent(Area parent)
    {
        this.parent = parent;
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

    public AreaContainer getNearestAncestorAreaContainer()
    {
        Area area = this.getParent();
        while (area != null && !(area is AreaContainer))
        {
            area = area.getParent();
        }
        return (AreaContainer)area;
    }

    public BorderAndPadding GetBorderAndPadding()
    {
        return bp;
    }

    public void addMarker(Marker marker)
    {
        markers.Add(marker);
    }

    public void addMarkers(ArrayList markers)
    {
        foreach (object o in markers)
        {
            this.markers.Add(o);
        }
    }

    public void addLineagePair(FObj fo, int areaPosition)
    {
        returnedBy.Add(fo, areaPosition);
    }

    public ArrayList getMarkers()
    {
        return markers;
    } 
}