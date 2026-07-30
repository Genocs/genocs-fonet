using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class Character : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new Character(parent, props));

    public const int OK = 0;

    public const int DOESNOT_FIT = 1;

    public Character(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:character";
    }



    public override Status Layout(Area area)
    {
        BlockArea blockArea;
        if (!(area is BlockArea))
        {
            FonetDriver.ActiveDriver.FireFonetWarning("Currently Character can only be in a BlockArea");
            return new Status(Status.OK);
        }
        blockArea = (BlockArea)area;
        bool textDecoration;

        AuralProps mAurProps = _propertyManager.GetAuralProps();
        BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        HyphenationProps mHyphProps = _propertyManager.GetHyphenationProps();
        MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();
        RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();
        ColorType c = this.Properties.GetProperty("color").GetColorType();
        float red = c.Red;
        float green = c.Green;
        float blue = c.Blue;

        int whiteSpaceCollapse =
            this.Properties.GetProperty("white-space-collapse").GetEnum();
        int wrapOption = this.Parent.Properties.GetProperty("wrap-option").GetEnum();

        int tmp = this.Properties.GetProperty("text-decoration").GetEnum();
        if (tmp == TextDecoration.UNDERLINE)
        {
            textDecoration = true;
        }
        else
        {
            textDecoration = false;
        }

        char characterValue = this.Properties.GetProperty("character").GetCharacter();
        string id = this.Properties.GetProperty("id").GetString();
        blockArea.GetIDReferences().InitializeID(id, blockArea);

        LineArea la = blockArea.getCurrentLineArea();
        if (la == null)
        {
            return new Status(Status.AREA_FULL_NONE);
        }
        la.changeFont(_propertyManager.GetFontState(area.GetFontInfo()));
        la.changeColor(red, green, blue);
        la.changeWrapOption(wrapOption);
        la.changeWhiteSpaceCollapse(whiteSpaceCollapse);
        blockArea.setupLinkSet(this.GetLinkSet());
        int result = la.addCharacter(characterValue, this.GetLinkSet(),
                                     textDecoration);
        if (result == Character.DOESNOT_FIT)
        {
            la = blockArea.createNextLineArea();
            if (la == null)
            {
                return new Status(Status.AREA_FULL_NONE);
            }
            la.changeFont(_propertyManager.GetFontState(area.GetFontInfo()));
            la.changeColor(red, green, blue);
            la.changeWrapOption(wrapOption);
            la.changeWhiteSpaceCollapse(whiteSpaceCollapse);
            blockArea.setupLinkSet(this.GetLinkSet());
            la.addCharacter(characterValue, this.GetLinkSet(),
                            textDecoration);
        }
        return new Status(Status.OK);
    }
}