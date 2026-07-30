using Genocs.Fonet.Layout;
using Genocs.Fonet.Layout.Inline;

namespace Genocs.Fonet.Fo;

internal class XMLElement : XMLObj
{
    // TODO: Implement namespace support for XML elements
    private readonly string _namespace = string.Empty;

    new internal class Maker : FObj.Maker
    {
        private readonly string _tag;
        private readonly Attributes _attributes;

        internal Maker(string tag, Attributes attributes)
        {
            _tag = tag;
            _attributes = attributes;
        }

        public override FObj Make(FObj parent, PropertyList propertyList)
            => new XMLElement(parent, propertyList, _tag, _attributes);
    }

    public static FObj.Maker GetMaker(string tag, Attributes attributes)
        => new Maker(tag, attributes);

    public XMLElement(FObj parent, PropertyList propertyList, string tag, Attributes attributes)
        : base(parent, propertyList, tag, attributes)
    {
    }

    public override Status Layout(Area area)
    {
        if (area is not ForeignObjectArea)
        {
            throw new FonetException("XML not in fo:instream-foreign-object");
        }

        return new Status(Status.OK);
    }
    public override string GetNameSpace()
        => _namespace;
}