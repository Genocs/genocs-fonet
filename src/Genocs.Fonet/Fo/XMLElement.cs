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

        internal Maker(string tag)
            => _tag = tag;

        public override FObj Make(FObj parent, PropertyList propertyList)
            => new XMLElement(parent, propertyList, _tag);
    }

    public static FObj.Maker GetMaker(string tag)
        => new Maker(tag);

    public XMLElement(FObj parent, PropertyList propertyList, string tag)
        : base(parent, propertyList, tag)
    {
        Init();
    }

    public override Status Layout(Area area)
    {
        if (area is not ForeignObjectArea)
        {
            throw new FonetException("XML not in fo:instream-foreign-object");
        }

        return new Status(Status.OK);
    }

    private void Init()
        => CreateBasicDocument();

    public override string GetNameSpace()
        => _namespace;
}