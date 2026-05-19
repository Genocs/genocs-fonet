using Genocs.Fonet.Fo;

namespace Genocs.Fonet.Fo.Properties;

internal class GenericMargin : LengthProperty.Maker
{
    protected GenericMargin(string name) : base(name) { }

    public override bool IsInherited()
    {
        return false;
    }

    public override Property GetShorthand(PropertyList propertyList)
    {
        ListProperty? listprop = (ListProperty?)propertyList.GetExplicitProperty("margin");
        if (listprop != null)
        {
            IShorthandParser shparser = new BoxPropShorthandParser(listprop);
            return shparser.GetValueForProperty(PropName, this, propertyList);
        }

        return null;
    }
}
