using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class TableAndCaption : FObj
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList) =>
            new TableAndCaption(parent, propertyList);
    }

    new public static FObj.Maker GetMaker() => new Maker();

    protected TableAndCaption(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        _name = "fo:table-and-caption";
    }

    public override Status Layout(Area area)
    {
        if (!_propertyManager.IsVisible())
        {
            return new Status(Status.OK);
        }

        Table? table = null;
        TableCaption? caption = null;
        foreach (FONode child in _children)
        {
            if (child is Table t)
            {
                table = t;
            }
            else if (child is TableCaption c)
            {
                caption = c;
            }
        }

        int captionSide = caption != null
            ? caption._properties.GetProperty("caption-side").GetEnum()
            : _properties.GetProperty("caption-side").GetEnum();
        bool captionFirst = captionSide != CaptionSide.AFTER;

        if (captionFirst && caption != null)
        {
            Status status = caption.Layout(area);
            if (status.IsIncomplete())
            {
                return status;
            }
        }

        if (table != null)
        {
            Status status = table.Layout(area);
            if (status.IsIncomplete())
            {
                return status;
            }
        }

        if (!captionFirst && caption != null)
        {
            Status status = caption.Layout(area);
            if (status.IsIncomplete())
            {
                return status;
            }
        }

        return new Status(Status.OK);
    }
}
