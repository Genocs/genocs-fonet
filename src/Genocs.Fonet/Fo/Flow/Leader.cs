using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class Leader : FObjMixed
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new Leader(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    public Leader(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:leader";
    }

    public override Status Layout(Area area)
    {
        BlockArea blockArea;
        if (!(area is BlockArea))
        {
            FonetDriver.ActiveDriver.FireFonetWarning(
                "fo:leader must be a direct child of fo:block ");
            return new Status(Status.OK);
        }
        else
        {
            blockArea = (BlockArea)area;
        }

        AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        AuralProps mAurProps = _propertyManager.GetAuralProps();
        BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();
        RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();
        ColorType c = this._properties.GetProperty("color").GetColorType();
        float red = c.Red;
        float green = c.Green;
        float blue = c.Blue;

        int leaderPattern = this._properties.GetProperty("leader-pattern").GetEnum();
        int leaderLengthOptimum =
            this._properties.GetProperty("leader-length.optimum").GetLength().MValue();
        int leaderLengthMinimum =
            this._properties.GetProperty("leader-length.minimum").GetLength().MValue();
        Length maxlength = this._properties.GetProperty("leader-length.maximum").GetLength();
        int leaderLengthMaximum;
        if (maxlength is PercentLength)
        {
            leaderLengthMaximum = (int)(((PercentLength)maxlength).Value()
                * area.getAllocationWidth());
        }
        else
        {
            leaderLengthMaximum = maxlength.MValue();
        }
        int ruleThickness =
            this._properties.GetProperty("rule-thickness").GetLength().MValue();
        int ruleStyle = this._properties.GetProperty("rule-style").GetEnum();
        int leaderPatternWidth =
            this._properties.GetProperty("leader-pattern-width").GetLength().MValue();
        int leaderAlignment =
            this._properties.GetProperty("leader-alignment").GetEnum();

        string id = this._properties.GetProperty("id").GetString();
        blockArea.GetIDReferences().InitializeID(id, blockArea);

        int succeeded = AddLeader(blockArea,
                                  _propertyManager.GetFontState(area.getFontInfo()),
                                  red, green, blue, leaderPattern,
                                  leaderLengthMinimum, leaderLengthOptimum,
                                  leaderLengthMaximum, ruleThickness,
                                  ruleStyle, leaderPatternWidth,
                                  leaderAlignment);
        if (succeeded == 1)
        {
            return new Status(Status.OK);
        }
        else
        {
            return new Status(Status.AREA_FULL_SOME);
        }
    }

    public int AddLeader(BlockArea ba, FontState fontState, float red,
                         float green, float blue, int leaderPattern,
                         int leaderLengthMinimum, int leaderLengthOptimum,
                         int leaderLengthMaximum, int ruleThickness,
                         int ruleStyle, int leaderPatternWidth,
                         int leaderAlignment)
    {
        LineArea la = ba.getCurrentLineArea();
        if (la == null)
        {
            return -1;
        }

        la.changeFont(fontState);
        la.changeColor(red, green, blue);

        if (leaderLengthOptimum <= (la.getRemainingWidth()))
        {
            la.AddLeader(leaderPattern, leaderLengthMinimum,
                         leaderLengthOptimum, leaderLengthMaximum, ruleStyle,
                         ruleThickness, leaderPatternWidth, leaderAlignment);
        }
        else
        {
            la = ba.createNextLineArea();
            if (la == null)
            {
                return -1;
            }
            la.changeFont(fontState);
            la.changeColor(red, green, blue);

            if (leaderLengthMinimum <= la.getContentWidth())
            {
                la.AddLeader(leaderPattern, leaderLengthMinimum,
                             leaderLengthOptimum, leaderLengthMaximum,
                             ruleStyle, ruleThickness, leaderPatternWidth,
                             leaderAlignment);
            }
            else
            {
                FonetDriver.ActiveDriver.FireFonetWarning(
                    "Leader doesn't fit into line, it will be clipped to fit.");
                la.AddLeader(leaderPattern, la.getRemainingWidth(),
                             leaderLengthOptimum, leaderLengthMaximum,
                             ruleStyle, ruleThickness, leaderPatternWidth,
                             leaderAlignment);
            }
        }
        return 1;
    }
}