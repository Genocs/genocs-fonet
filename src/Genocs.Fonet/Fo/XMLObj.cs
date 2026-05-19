using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Layout;
using System.Collections;
using System.Xml;

namespace Genocs.Fonet.Fo;

internal abstract class XMLObj(FObj parent, PropertyList propertyList, string tag) : FObj(parent, propertyList)
{
    protected string _tagName = tag;

    protected XmlNode _element;

    protected XmlDocument? _doc;

    protected const string NS = "http://www.genocs.com/fonet";

    public abstract string GetNameSpace();

    protected static Hashtable ns = new Hashtable();

    public void AddGraphic(XmlDocument doc, XmlNode parent)
        => _doc = doc;

    public void BuildTopLevel(XmlDocument doc, XmlNode svgRoot) { }

    public XmlDocument CreateBasicDocument()
    {
        try
        {
            _doc = new XmlDocument();
            _doc.AppendChild(_doc.CreateElement("graph", NS));
            _element = _doc.DocumentElement;
            BuildTopLevel(_doc, _element);
        }
        catch (Exception e)
        {
            FonetDriver.ActiveDriver.FireFonetError(e.ToString());
        }
        return _doc;
    }

    protected internal override void AddChild(FONode child)
    {
        if (child is XMLObj obj)
        {
            obj.AddGraphic(_doc, _element);
        }
    }

    protected internal override void AddCharacters(char[] data, int start, int length)
    {
        string str = new string(data, start, length - start);
        _doc.DocumentElement.AppendChild(_doc.CreateTextNode(str));
    }

    public override Status Layout(Area area)
    {
        FonetDriver.ActiveDriver.FireFonetError($"{_name} outside foreign xml");
        return new Status(Status.OK);
    }

    public override void RemoveID(IDReferences idReferences) { }
    public override void SetIsInTableCell() { }
    public override void ForceStartOffset(int offset) { }
    public override void ForceWidth(int width) { }
    public override void ResetMarker() { }
    public override void SetLinkSet(LinkSet linkSet) { }
    public override ArrayList GetMarkerSnapshot(ArrayList snapshot)
        => snapshot;

    public override void Rollback(ArrayList snapshot) { }
    protected override void SetWritingMode() { }
}