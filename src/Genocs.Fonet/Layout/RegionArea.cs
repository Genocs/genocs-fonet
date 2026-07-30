using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Layout;

internal class RegionArea(int xPosition, int yPosition, int width, int height)
{
    protected int xPosition = xPosition;
    protected int yPosition = yPosition;
    protected int width = width;
    protected int height = height;
    protected BackgroundProps? background;

    public AreaContainer MakeAreaContainer()
    {
        AreaContainer area = new(null, xPosition, yPosition, width, height, Position.ABSOLUTE, null);
        area.setBackground(getBackground());
        return area;
    }

    public BackgroundProps? getBackground()
    {
        return this.background;
    }

    public void setBackground(BackgroundProps bg)
    {
        this.background = bg;
    }

    public int GetHeight()
    {
        return height;
    }
}