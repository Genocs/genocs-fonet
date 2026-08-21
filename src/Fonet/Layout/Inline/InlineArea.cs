namespace Genocs.Fonet.Layout.Inline;

internal abstract class InlineArea : Area
{
    public float Red { get; }
    public float Green { get; }
    public float Blue { get; }

    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
    protected int height = 0;
    private int verticalAlign = 0;
    protected string? pageNumberId;

    protected bool underlined = false;
    protected bool overlined = false;
    protected bool lineThrough = false;

    public InlineArea(FontState fontState, int width, float red, float green, float blue)
        : base(fontState, null)
    {
        this.contentRectangleWidth = width;
        Red = red;
        Green = green;
        Blue = blue;
    }

    public override void SetHeight(int height)
    {
        this.height = height;
    }

    public override int GetHeight()
    {
        return this.height;
    }

    public virtual void setVerticalAlign(int align)
    {
        this.verticalAlign = align;
    }

    public virtual int getVerticalAlign()
    {
        return this.verticalAlign;
    }

    public virtual int getXOffset()
    {
        return OffsetX;
    }

    public string getPageNumberID()
    {
        return pageNumberId;
    }

    public void setUnderlined(bool ul)
    {
        this.underlined = ul;
    }

    public bool getUnderlined()
    {
        return this.underlined;
    }

    public void setOverlined(bool ol)
    {
        this.overlined = ol;
    }

    public bool getOverlined()
    {
        return this.overlined;
    }

    public void setLineThrough(bool lt)
    {
        this.lineThrough = lt;
    }

    public bool getLineThrough()
    {
        return this.lineThrough;
    }
}