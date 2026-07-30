using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Tests;

public class Phase3ClearTests
{
    [Fact]
    public void GetClearOffset_MovesBelowActiveLeftFloat()
    {
        var column = new ColumnArea(null!, 0, 100000, 200000, 100000, 1);
        var sideFloat = new SideFloatArea(null!, 0, 90000, 50000, 30000, FloatAlign.LEFT);
        column.RegisterSideFloat(FloatAlign.LEFT, yStart: 10000, width: 50000, height: 30000, sideFloat);

        int offset = column.GetClearOffset(anchorY: 10000, Clear.LEFT);

        Assert.Equal(30000, offset);
    }

    [Fact]
    public void GetClearOffset_ReturnsZeroWhenNoMatchingFloat()
    {
        var column = new ColumnArea(null!, 0, 100000, 200000, 100000, 1);
        var sideFloat = new SideFloatArea(null!, 0, 90000, 50000, 30000, FloatAlign.LEFT);
        column.RegisterSideFloat(FloatAlign.LEFT, yStart: 10000, width: 50000, height: 30000, sideFloat);

        int offset = column.GetClearOffset(anchorY: 50000, Clear.LEFT);

        Assert.Equal(0, offset);
    }

    [Fact]
    public void GetClearOffset_BothClearsLeftAndRightFloats()
    {
        var column = new ColumnArea(null!, 0, 100000, 200000, 100000, 1);
        var leftFloat = new SideFloatArea(null!, 0, 90000, 50000, 20000, FloatAlign.LEFT);
        var rightFloat = new SideFloatArea(null!, 150000, 90000, 40000, 40000, FloatAlign.RIGHT);
        column.RegisterSideFloat(FloatAlign.LEFT, yStart: 10000, width: 50000, height: 20000, leftFloat);
        column.RegisterSideFloat(FloatAlign.RIGHT, yStart: 10000, width: 40000, height: 40000, rightFloat);

        int offset = column.GetClearOffset(anchorY: 10000, Clear.BOTH);

        Assert.Equal(40000, offset);
    }
}
