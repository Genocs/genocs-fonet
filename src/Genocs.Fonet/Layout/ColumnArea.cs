using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet.Layout;

internal class ColumnArea : AreaContainer
{
    private const int FloatGap = 1000;

    private int columnIndex;
    private int maxColumns;
    private readonly List<SideFloatRecord> _sideFloats = [];

    public ColumnArea(FontState fontState, int xPosition, int yPosition, int allocationWidth, int maxHeight, int columnCount)
        : base(fontState, xPosition, yPosition, allocationWidth, maxHeight, Fo.Properties.Position.ABSOLUTE, null)
    {
        this.maxColumns = columnCount;
        this.setAreaName("normal-flow-ref.-area");
    }

    public override void Render(PdfRenderer renderer)
    {
        renderer.RenderAreaContainer(this);
    }

    public override void end()
    {
    }

    public override void start()
    {
    }

    public override int spaceLeft()
    {
        return maxHeight - currentHeight;
    }

    public int getColumnIndex()
    {
        return columnIndex;
    }

    public void setColumnIndex(int columnIndex)
    {
        this.columnIndex = columnIndex;
    }

    public void incrementSpanIndex()
    {
        SpanArea span = (SpanArea)this.Parent;
        span.setCurrentColumn(span.getCurrentColumn() + 1);
    }

    internal void RegisterSideFloat(int side, int yStart, int width, int height, SideFloatArea area)
    {
        _sideFloats.Add(new SideFloatRecord(side, yStart, width, height, area));
    }

    internal void GetFloatIndents(int yPosition, int lineHeight, out int extraStartIndent, out int extraEndIndent)
    {
        extraStartIndent = 0;
        extraEndIndent = 0;

        foreach (SideFloatRecord sideFloat in _sideFloats)
        {
            if (!sideFloat.Overlaps(yPosition, lineHeight))
            {
                continue;
            }

            if (sideFloat.Side == FloatAlign.LEFT)
            {
                extraStartIndent = Math.Max(extraStartIndent, sideFloat.Width + FloatGap);
            }
            else if (sideFloat.Side == FloatAlign.RIGHT)
            {
                extraEndIndent = Math.Max(extraEndIndent, sideFloat.Width + FloatGap);
            }
        }
    }

    internal int GetClearOffset(int anchorY, int clear)
    {
        if (clear == Clear.NONE)
        {
            return 0;
        }

        int targetY = anchorY;

        foreach (SideFloatRecord sideFloat in _sideFloats)
        {
            if (!MatchesClearSide(sideFloat.Side, clear))
            {
                continue;
            }

            if (sideFloat.YStart <= anchorY && sideFloat.YEnd > targetY)
            {
                targetY = sideFloat.YEnd;
            }
        }

        return targetY - anchorY;
    }

    private static bool MatchesClearSide(int floatSide, int clear)
        => clear == Clear.BOTH
            || (clear == Clear.LEFT && floatSide == FloatAlign.LEFT)
            || (clear == Clear.RIGHT && floatSide == FloatAlign.RIGHT);

    internal static ColumnArea? FindColumnArea(Area? area)
    {
        while (area != null)
        {
            if (area is ColumnArea columnArea)
            {
                return columnArea;
            }

            area = area.Parent;
        }

        return null;
    }
}