using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;


internal class Character : FObj
{
    public const int OK = 0;

    public const int DOESNOT_FIT = 1;

    public Character(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:character";
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new Character(parent, propertyList);
        }
    }

    public override Status Layout(Area area)
    {
        BlockArea blockArea;
        if (!(area is BlockArea))
        {
            FonetDriver.ActiveDriver.FireFonetWarning(
                "Currently Character can only be in a BlockArea");
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
        ColorType c = this._properties.GetProperty("color").GetColorType();
        float red = c.Red;
        float green = c.Green;
        float blue = c.Blue;

        int whiteSpaceCollapse =
            this._properties.GetProperty("white-space-collapse").GetEnum();
        int wrapOption = this._parent._properties.GetProperty("wrap-option").GetEnum();

        int tmp = this._properties.GetProperty("text-decoration").GetEnum();
        if (tmp == TextDecoration.UNDERLINE)
        {
            textDecoration = true;
        }
        else
        {
            textDecoration = false;
        }

        char characterValue = this._properties.GetProperty("character").GetCharacter();
        string id = this._properties.GetProperty("id").GetString();
        blockArea.GetIDReferences().InitializeID(id, blockArea);

        LineArea la = blockArea.getCurrentLineArea();
        if (la == null)
        {
            return new Status(Status.AREA_FULL_NONE);
        }
        la.changeFont(_propertyManager.GetFontState(area.getFontInfo()));
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
            la.changeFont(_propertyManager.GetFontState(area.getFontInfo()));
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