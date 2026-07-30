using System.Collections;
using Genocs.Fonet.Fo;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Tests;

public class Phase3ZIndexTests
{
    [Fact]
    public void GetChildrenInZOrder_SortsAscendingAndPreservesStableOrder()
    {
        var children = new ArrayList
        {
            CreateArea(zIndex: 2000),
            CreateArea(zIndex: 1000),
            CreateArea(zIndex: 2000),
        };

        var ordered = Area.GetChildrenInZOrder(children);

        Assert.Equal(1000, ((Area)ordered[0]).ZIndex);
        Assert.Equal(2000, ((Area)ordered[1]).ZIndex);
        Assert.Equal(2000, ((Area)ordered[2]).ZIndex);
        Assert.Same(children[1], ordered[0]);
        Assert.Same(children[0], ordered[1]);
        Assert.Same(children[2], ordered[2]);
    }

    [Fact]
    public void ZIndexMaker_ParsesAutoValue()
    {
        var maker = ZIndexMaker.Maker("z-index");

        var auto = maker.ConvertProperty(new StringProperty("auto"), null!, null!);
        Assert.NotNull(auto);
        Assert.True(auto.GetLength()!.IsAuto());
    }

    private static SideFloatArea CreateArea(int zIndex)
        => new(null!, 0, 0, 1000, 1000, FloatAlign.LEFT)
        {
            ZIndex = zIndex,
        };
}
