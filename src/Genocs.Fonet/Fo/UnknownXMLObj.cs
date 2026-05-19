using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo;

internal class UnknownXMLObj : XMLObj
{
    private readonly string _namespace;

    new internal class Maker : FObj.Maker
    {
        private readonly string _space;
        private readonly string _tag;

        internal Maker(string space, string tag)
        {
            _space = space;
            _tag = tag;
        }

        public override FObj Make(FObj parent, PropertyList propertyList)
            => new UnknownXMLObj(parent, propertyList, _space, _tag);
    }

    public static FObj.Maker GetMaker(string space, string tag)
        => new Maker(space, tag);

    protected UnknownXMLObj(FObj parent, PropertyList propertyList, string nspace, string tag)
        : base(parent, propertyList, tag)
    {
        _namespace = nspace;

        if (!"".Equals(_namespace))
        {
            _name = $"{_namespace}:{tag}";
        }
        else
        {
            _name = $"(none):{tag}";
        }
    }

    public override string GetNameSpace()
        => _namespace;

    protected internal override void AddChild(FONode child)
    {
        if (_doc == null)
        {
            CreateBasicDocument();
        }
        base.AddChild(child);
    }

    protected internal override void AddCharacters(char[] data, int start, int length)
    {
        if (_doc == null)
        {
            CreateBasicDocument();
        }
        base.AddCharacters(data, start, length);
    }

    public override Status Layout(Area area)
        => new(Status.OK);
}