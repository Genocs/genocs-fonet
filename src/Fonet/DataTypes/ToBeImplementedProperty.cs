using Genocs.Fonet.Fo;

namespace Genocs.Fonet.DataTypes;

internal class ToBeImplementedProperty : Property
{
    internal class Maker(string propName) : PropertyMaker(propName)
    {
        public override Property ConvertProperty(Property p, PropertyList propertyList, FObj fo)
        {
            if (p is ToBeImplementedProperty)
            {
                return p;
            }

            ToBeImplementedProperty val = new(PropertyName);
            return val;
        }
    }

    public ToBeImplementedProperty(string propName)
    {
        FonetDriver.ActiveDriver?.FireFonetWarning($"property - \"{propName}\" is not implemented yet.");
    }
}