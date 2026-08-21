namespace Genocs.Fonet.Pdf.Gdi.Font.Tables;

/// <summary>
/// Summary description for HorizontalMetric.
/// </summary>
internal class HorizontalMetric(ushort advanceWidth, short leftSideBearing)
{
    public HorizontalMetric Clone()
    {
        return new HorizontalMetric(advanceWidth, leftSideBearing);
    }

    public ushort AdvanceWidth
    {
        get { return advanceWidth; }
    }

    public short LeftSideBearing
    {
        get { return leftSideBearing; }
    }
}