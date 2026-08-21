using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class PageNumberCitation : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new PageNumberCitation(parent, props));

    private float red;
    private float green;
    private float blue;
    private int wrapOption;
    private int whiteSpaceCollapse;
    private Area area;
    private string pageNumber;
    private string refId;
    private string id;
    private TextState ts;

    public PageNumberCitation(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:page-number-citation";
    }

    public override Status Layout(Area area)
    {
        if (area is not BlockArea)
        {
            FonetDriver.ActiveDriver?.FireFonetWarning("Page-number-citation outside block area");
            return new Status(Status.OK);
        }

        IDReferences idReferences = area.GetIDReferences();
        this.area = area;
        if (this._marker == MarkerStart)
        {
            AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
            AuralProps mAurProps = _propertyManager.GetAuralProps();
            BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
            BackgroundProps bProps = _propertyManager.GetBackgroundProps();
            MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();
            RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

            ColorType c = this.Properties.GetProperty("color").GetColorType();
            this.red = c.Red;
            this.green = c.Green;
            this.blue = c.Blue;

            this.wrapOption = this.Properties.GetProperty("wrap-option").GetEnum();
            this.whiteSpaceCollapse = this.Properties.GetProperty("white-space-collapse").GetEnum();
            this.refId = this.Properties.GetProperty("ref-id").GetString();

            if (this.refId.Equals(""))
            {
                throw new FonetException("page-number-citation must contain \"ref-id\"");
            }

            this.id = this.Properties.GetProperty("id").GetString();
            idReferences.CreateID(id);
            ts = new TextState();

            this._marker = 0;
        }

        if (_marker == 0)
        {
            idReferences.ConfigureID(id, area);
        }


        pageNumber = idReferences.SetPageNumber(refId);

        if (pageNumber != null)
        {
            this._marker =
                FOText.addText((BlockArea)area,
                               _propertyManager.GetFontState(area.GetFontInfo()), red,
                               green, blue, wrapOption, null,
                               whiteSpaceCollapse, pageNumber.ToCharArray(),
                               0, pageNumber.Length, ts,
                               VerticalAlign.BASELINE);
        }
        else
        {
            BlockArea blockArea = (BlockArea)area;
            LineArea la = blockArea.getCurrentLineArea();
            if (la == null)
            {
                return new Status(Status.AREA_FULL_NONE);
            }
            la.changeFont(_propertyManager.GetFontState(area.GetFontInfo()));
            la.changeColor(red, green, blue);
            la.changeWrapOption(wrapOption);
            la.changeWhiteSpaceCollapse(whiteSpaceCollapse);
            la.addPageNumberCitation(refId, null);
            this._marker = -1;
        }

        if (this._marker == -1)
        {
            return new Status(Status.OK);
        }
        else
        {
            return new Status(Status.AREA_FULL_NONE);
        }
    }
}