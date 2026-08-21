using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet.Layout.Inline;

internal class LeaderArea : InlineArea
{
    private int ruleThickness;
    private int leaderLengthOptimum;
    private int leaderPattern;
    private int ruleStyle;

    public LeaderArea(
        FontState fontState, float red, float green,
        float blue, string text, int leaderLengthOptimum,
        int leaderPattern, int ruleThickness, int ruleStyle)
        : base(fontState, leaderLengthOptimum, red, green, blue)
    {
        this.leaderPattern = leaderPattern;
        this.leaderLengthOptimum = leaderLengthOptimum;
        this.ruleStyle = ruleStyle;
        if (ruleStyle == RuleStyle.NONE)
        {
            ruleThickness = 0;
        }

        this.ruleThickness = ruleThickness;
    }

    public override void Render(PdfRenderer renderer)
    {
        renderer.RenderLeaderArea(this);
    }

    public int getRuleThickness()
    {
        return this.ruleThickness;
    }

    public int getRuleStyle()
    {
        return this.ruleStyle;
    }

    public int getLeaderPattern()
    {
        return this.leaderPattern;
    }

    public int getLeaderLength()
    {
        return this.contentRectangleWidth;
    }
}