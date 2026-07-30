using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo;

internal class UnknownXMLObj : XMLObj
{
    private readonly string _namespace;

    new internal class Maker : FObj.Maker
    {
        private readonly string _space;
        private readonly string _tag;
        private readonly Attributes _attributes;

        internal Maker(string space, string tag, Attributes attributes)
        {
            _space = space;
            _tag = tag;
            _attributes = attributes;
        }

        public override FObj Make(FObj parent, PropertyList propertyList)
            => new UnknownXMLObj(parent, propertyList, _space, _tag, _attributes);
    }

    public static FObj.Maker CreateMaker(string space, string tag, Attributes attributes)
        => new Maker(space, tag, attributes);

    protected UnknownXMLObj(FObj parent, PropertyList propertyList, string nameSpace, string tag, Attributes attributes)
        : base(parent, propertyList, tag, attributes)
    {
        _namespace = nameSpace;

        if (!"".Equals(_namespace))
        {
            Name = $"{_namespace}:{tag}";
        }
        else
        {
            Name = $"(none):{tag}";
        }
    }

    public override string GetNameSpace()
        => _namespace;

    public override Status Layout(Area area)
        => new(Status.OK);
}