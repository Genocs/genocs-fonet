using System.Collections;

namespace Genocs.Fonet.Fo;

internal class ListProperty(Property prop) : Property
{
    internal class Maker(string name) : PropertyMaker(name)
    {
        public override Property ConvertProperty(Property p, PropertyList propertyList, FObj fo)
        {
            if (p is ListProperty)
            {
                return p;
            }
            else
            {
                return new ListProperty(p);
            }
        }

    }

    protected ArrayList? _list = [prop];

    public void AddProperty(Property prop)
        => _list.Add(prop);

    public override ArrayList GetList()
        => _list;

    public override object GetObject()
        => _list;
}