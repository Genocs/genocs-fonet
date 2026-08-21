using Genocs.Fonet.Fo.Pagination;
using System.Collections;
using System.Xml;

namespace Genocs.Fonet.Fo;

/// <summary>
/// Builds the formatting object tree.
/// </summary>
internal sealed class FOTreeBuilder
{
    /// <summary>
    /// Table mapping element names to the makers of objects
    /// representing formatting objects.
    /// </summary>
    private Dictionary<string, Dictionary<string, FObj.Maker>> fobjTable = [];

    private ArrayList namespaces = [];

    /// <summary>
    /// Class that builds a property list for each formatting object.
    /// </summary>
    private readonly Hashtable _propertylistTable = [];

    /// <summary>
    /// Current formatting object being handled.
    /// </summary>
    private FObj? currentFObj;

    /// <summary>
    /// The root of the formatting object tree.
    /// </summary>
    private FObj? _rootFObj;

    /// <summary>
    /// Set of names of formatting objects encountered but unknown.
    /// </summary>
    private readonly Hashtable _unknownFOs = [];

    /// <summary>
    /// The class that handles formatting and rendering to a stream.
    /// </summary>
    private StreamRenderer? _streamRenderer;

    internal FOTreeBuilder() { }

    /// <summary>
    /// Sets the stream renderer that will be used as output.
    /// </summary>
    internal void SetStreamRenderer(StreamRenderer streamRenderer)
    {
        _streamRenderer = streamRenderer;
    }

    /// <summary>
    /// Add a mapping from element name to maker.
    /// </summary>
    internal void AddElementMapping(string namespaceURI, Dictionary<string, FObj.Maker> table)
    {
        fobjTable.Add(namespaceURI, table);
        namespaces.Add(String.Intern(namespaceURI));
    }

    /// <summary>
    /// Add a mapping from property name to maker.
    /// </summary>
    internal void AddPropertyMapping(string namespaceURI, Hashtable list)
    {
        var propertylist = (PropertyListBuilder?)_propertylistTable[namespaceURI];
        if (propertylist == null)
        {
            propertylist = new PropertyListBuilder();
            propertylist.AddList(list);
            _propertylistTable.Add(namespaceURI, propertylist);
        }
        else
        {
            propertylist.AddList(list);
        }
    }

    private FObj.Maker? GetFObjMaker(string uri, string localName)
    {
        if (!fobjTable.TryGetValue(uri, out Dictionary<string, FObj.Maker>? table))
        {
            return null;
        }

        return table.TryGetValue(localName, out FObj.Maker? maker) ? maker : null;
    }

    private void StartElement(string uri, string localName, Attributes attlist)
    {
        FObj fobj;

        FObj.Maker? fobjMaker = GetFObjMaker(uri, localName);

        var currentListBuilder = (PropertyListBuilder?)_propertylistTable[uri];

        bool foreignXML = false;
        if (fobjMaker == null)
        {
            string fullName = $"{uri}^{localName}";

            // Foreign namespaces are allowed inside fo:instream-foreign-object.
            bool isForeignObjectContext = currentFObj is Flow.InstreamForeignObject || currentFObj is XMLObj;

            if (!isForeignObjectContext && !_unknownFOs.ContainsKey(fullName))
            {
                _unknownFOs.Add(fullName, "");
                FonetDriver.ActiveDriver?.FireFonetError($"Unknown formatting object {fullName}");
            }

            if (namespaces.Contains(String.Intern(uri)))
            {
                fobjMaker = Unknown.CreateMaker();
            }
            else
            {
                fobjMaker = new UnknownXMLObj.Maker(uri, localName, attlist);
                foreignXML = true;
            }
        }

        PropertyList? list = null;
        if (currentListBuilder != null)
        {
            list = currentListBuilder.MakeList(uri, localName, attlist, currentFObj);
        }
        else if (foreignXML)
        {
            if (currentFObj == null)
            {
                throw new FonetException("Foreign XML is only valid inside a formatting object context");
            }

            // Reuse parent properties since foreign XML nodes do not have FO property mappings.
            list = currentFObj.Properties;
        }
        else
        {
            if (currentFObj == null)
            {
                throw new FonetException("Invalid XML or missing namespace");
            }
            list = currentFObj.Properties;
        }

        fobj = fobjMaker.Make(currentFObj, list);

        if (_rootFObj == null)
        {
            _rootFObj = fobj;
            if (!fobj.Name.Equals("fo:root"))
            {
                throw new FonetException($"Root element must be root, not {fobj.Name}");
            }
        }
        else if (!(fobj is PageSequence))
        {
            currentFObj.AddChild(fobj);
        }

        currentFObj = fobj;
    }

    private void EndElement()
    {
        if (currentFObj != null)
        {
            currentFObj.End();

            // If it is a page-sequence, then we can finally Render it.
            // This is the biggest performance problem we have, we need
            // to be able to Render prior to this point.
            if (currentFObj is PageSequence)
            {
                _streamRenderer.Render((PageSequence)currentFObj);

            }

            currentFObj = currentFObj.Parent;
        }
    }

    internal void Parse(XmlReader reader)
    {
        int buflen = 500;
        char[] buffer = new char[buflen];
        try
        {
            object nsuri = reader.NameTable.Add("http://www.w3.org/2000/xmlns/");

            FonetDriver.ActiveDriver?.FireFonetInfo("Building formatting object tree");
            _streamRenderer?.StartRenderer();

            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        Attributes atts = new Attributes();
                        while (reader.MoveToNextAttribute())
                        {
                            if (!reader.NamespaceURI.Equals(nsuri))
                            {
                                SaxAttribute newAtt = new SaxAttribute();
                                newAtt.Name = reader.Name;
                                newAtt.NamespaceURI = reader.NamespaceURI;
                                newAtt.Value = reader.Value;
                                atts.attArray.Add(newAtt);
                            }
                        }
                        reader.MoveToElement();
                        StartElement(reader.NamespaceURI, reader.LocalName, atts.TrimArray());

                        if (reader.IsEmptyElement)
                        {
                            EndElement();
                        }
                        break;
                    case XmlNodeType.EndElement:
                        EndElement();
                        break;
                    case XmlNodeType.Text:
                        char[] chars = reader.ReadString().ToCharArray();
                        currentFObj?.AddCharacters(chars, 0, chars.Length);

                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            goto case XmlNodeType.Element;
                        }

                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            goto case XmlNodeType.EndElement;
                        }

                        break;
                    default:
                        break;
                }
            }

            FonetDriver.ActiveDriver?.FireFonetInfo($"Parsing completed in [{sw.Elapsed.TotalSeconds}] seconds.");

            FonetDriver.ActiveDriver?.FireFonetInfo("Parsing of document complete, stopping renderer.");
            _streamRenderer?.StopRenderer();
        }
        catch (Exception exception)
        {
            FonetDriver.ActiveDriver?.FireFonetError(exception.ToString());
        }
        finally
        {
            if (reader.ReadState != ReadState.Closed)
            {
                reader.Close();
            }
        }
    }
}

internal class Attributes
{
    internal ArrayList attArray = new ArrayList(3);

    // called by property list builder
    internal int getLength()
    {
        return attArray.Count;
    }

    // called by property list builder
    internal string getQName(int index)
    {
        SaxAttribute saxAtt = (SaxAttribute)attArray[index];
        return saxAtt.Name;
    }

    // called by property list builder
    internal string getValue(int index)
    {
        SaxAttribute saxAtt = (SaxAttribute)attArray[index];
        return saxAtt.Value;
    }

    // called by property list builder
    internal string? getValue(string name)
    {
        foreach (SaxAttribute att in attArray)
        {
            if (att.Name.Equals(name))
            {
                return att.Value;
            }
        }
        return null;
    }

    // only called above
    internal Attributes TrimArray()
    {
        attArray.TrimToSize();
        return this;
    }
}

// Only used by FO tree builder
internal record struct SaxAttribute(string Name, string NamespaceURI, string Value);
