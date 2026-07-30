using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Layout;
using System.Collections;
using System.Xml;

namespace Genocs.Fonet.Fo;

internal abstract class XMLObj : FObj
{
    protected readonly string _tagName;

    protected readonly Attributes _attributes;

    protected XmlNode? _element;

    protected XmlDocument? _doc;

    protected const string NS = "http://www.genocs.com/fonet";

    protected XMLObj(FObj parent, PropertyList propertyList, string tag, Attributes attributes)
        : base(parent, propertyList)
    {
        _tagName = tag;
        _attributes = attributes;
        CreateStandaloneDocument();
    }

    public abstract string GetNameSpace();

    protected static Hashtable ns = new Hashtable();

    public XmlDocument? GetDocument()
        => _doc;

    public void AddGraphic(XmlDocument doc, XmlNode parent)
    {
        _doc = doc;
        _element = CreateElement(doc);
        parent.AppendChild(_element);
    }

    public void BuildTopLevel(XmlDocument doc, XmlNode svgRoot) { }

    public XmlDocument CreateBasicDocument()
    {
        try
        {
            _doc = new XmlDocument();
            _element = CreateElement(_doc);
            _doc.AppendChild(_element);
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
            if (_doc == null || _element == null)
            {
                CreateStandaloneDocument();
            }

            obj.AddGraphic(_doc!, _element!);
        }

        _children.Add(child);
    }

    protected internal override void AddCharacters(char[] data, int start, int length)
    {
        if (_doc == null || _element == null || length <= 0)
        {
            return;
        }

        string str = new string(data, start, length);
        _element.AppendChild(_doc.CreateTextNode(str));
    }

    public override Status Layout(Area area)
    {
        FonetDriver.ActiveDriver?.FireFonetError($"{Name} outside foreign xml");
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

    private void CreateStandaloneDocument()
    {
        _doc = new XmlDocument();
        _element = CreateElement(_doc);
        _doc.AppendChild(_element);
    }

    private XmlElement CreateElement(XmlDocument doc)
    {
        XmlElement element = string.IsNullOrEmpty(GetNameSpace())
            ? doc.CreateElement(_tagName)
            : doc.CreateElement(_tagName, GetNameSpace());

        foreach (SaxAttribute attribute in _attributes.attArray)
        {
            if (string.IsNullOrEmpty(attribute.NamespaceURI))
            {
                element.SetAttribute(attribute.Name, attribute.Value);
                continue;
            }

            element.SetAttribute(attribute.Name, attribute.NamespaceURI, attribute.Value);
        }

        return element;
    }
}