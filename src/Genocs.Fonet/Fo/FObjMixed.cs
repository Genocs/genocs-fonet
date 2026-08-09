using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo;

internal class FObjMixed : FObj
{
    /// <summary>
    /// TODO: The text state is not set in the current implementation.
    /// It should be set based on the properties of the parent element or the current element.
    /// </summary>
    protected TextState? _textState;

    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj? parent, PropertyList propertyList)
        {
            return new FObjMixed(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    protected FObjMixed(FObj? parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
    }

    public TextState? GetTextState()
        => _textState;

    protected internal override void AddCharacters(char[] data, int start, int length)
    {
        FOText ft = new FOText(data, start, length, this);
        ft.setUnderlined(_textState?.getUnderlined()?? false);
        ft.setOverlined(_textState?.getOverlined()?? false);
        ft.setLineThrough(_textState?.getLineThrough()?? false);
        AddChild(ft);
    }

    public override Status Layout(Area area)
    {
        if (Properties != null && !_propertyManager.IsVisible())
        {
            return new Status(Status.OK);
        }

        if (Properties != null)
        {
            Property? prop = Properties.GetProperty("id");
            if (prop != null)
            {
                string? id = prop.GetString();

                if (_marker == MarkerStart)
                {
                    area.GetIDReferences()?.CreateID(id);
                    _marker = 0;
                }

                if (_marker == 0)
                {
                    area.GetIDReferences()?.ConfigureID(id, area);
                }
            }
        }

        int numChildren = _children.Count;
        for (int i = _marker; i < numChildren; i++)
        {
            FONode fo = (FONode)_children[i];
            Status status;
            if ((status = fo.Layout(area)).IsIncomplete())
            {
                _marker = i;
                return status;
            }
        }

        return new Status(Status.OK);
    }
}