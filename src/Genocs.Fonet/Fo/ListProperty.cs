using System.Collections;

namespace Genocs.Fonet.Fo;

internal class ListProperty(Property property) : Property
{
    internal class Maker(string name) : PropertyMaker(name)
    {
        public override Property ConvertProperty(Property property, PropertyList propertyList, FObj? fo)
        {
            if (property is ListProperty)
            {
                return property;
            }
            else
            {
                return new ListProperty(property);
            }
        }

    }

    protected ArrayList? _list = [property];

    public void AddProperty(Property prop)
        => _list.Add(prop);

    public override ArrayList GetList()
        => _list;

    public override object GetObject()
        => _list;
}