using Genocs.Fonet.Fo.Properties;

namespace Genocs.Fonet.Layout;

internal class BodyRegionArea(int xPosition, int yPosition, int width, int height) : RegionArea(xPosition, yPosition, width, height)
{
    private int columnCount;
    private int columnGap;

    public BodyAreaContainer makeBodyAreaContainer()
    {
        BodyAreaContainer area = new BodyAreaContainer(
            null, xPosition, yPosition, width,
            height, Position.ABSOLUTE, columnCount, columnGap);
        area.setBackground(getBackground());
        return area;
    }

    public void setColumnCount(int columnCount)
    {
        this.columnCount = columnCount;
    }

    public int getColumnCount()
    {
        return columnCount;
    }

    public void setColumnGap(int columnGap)
    {
        this.columnGap = columnGap;
    }

    public int getColumnGap()
    {
        return columnGap;
    }

}