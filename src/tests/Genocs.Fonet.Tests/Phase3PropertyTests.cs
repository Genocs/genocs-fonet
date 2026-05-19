using Genocs.Fonet.Fo;
using Genocs.Fonet.Fo.Properties;

namespace Genocs.Fonet.Tests;

public class Phase3PropertyTests
{
    [Fact]
    public void VisibilityMaker_ParsesEnumValues()
    {
        var maker = VisibilityMaker.Maker("visibility");
        Assert.Equal(Visibility.VISIBLE, maker.CheckEnumValues("visible")!.GetEnum());
        Assert.Equal(Visibility.HIDDEN, maker.CheckEnumValues("hidden")!.GetEnum());
        Assert.Equal(Visibility.COLLAPSE, maker.CheckEnumValues("collapse")!.GetEnum());
    }

    [Fact]
    public void CaptionSideMaker_ParsesEnumValues()
    {
        var maker = CaptionSideMaker.Maker("caption-side");
        Assert.Equal(CaptionSide.BEFORE, maker.CheckEnumValues("before")!.GetEnum());
        Assert.Equal(CaptionSide.AFTER, maker.CheckEnumValues("after")!.GetEnum());
    }

    [Fact]
    public void FloatMaker_ParsesEnumValues()
    {
        var maker = FloatMaker.Maker("float");
        Assert.Equal(FloatAlign.NONE, maker.CheckEnumValues("none")!.GetEnum());
        Assert.Equal(FloatAlign.LEFT, maker.CheckEnumValues("left")!.GetEnum());
        Assert.Equal(FloatAlign.RIGHT, maker.CheckEnumValues("right")!.GetEnum());
    }

    [Fact]
    public void ClearMaker_ParsesEnumValues()
    {
        var maker = ClearMaker.Maker("clear");
        Assert.Equal(Clear.BOTH, maker.CheckEnumValues("both")!.GetEnum());
    }

    [Fact]
    public void MarginMaker_ConvertsShorthandList()
    {
        var maker = MarginMaker.Maker("margin");
        var list = new ListProperty(new StringProperty("10pt"));
        list.AddProperty(new StringProperty("20pt"));
        var converted = maker.ConvertProperty(list, null!, null!);
        Assert.IsType<ListProperty>(converted);
    }
}
